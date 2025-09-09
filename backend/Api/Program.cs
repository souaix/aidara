using Backend.Application.Listings;
using Backend.Application.Ports;
using Backend.Application.Users;
using Backend.Application.Wallet;
using Backend.Domain.Wallet;
using Backend.Infrastructure.Persistence.Postgres;
using Npgsql;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ========= 連線字串 =========
var connString = builder.Configuration.GetConnectionString("Postgres");

// ========= Npgsql DataSource (9.x 正確寫法) =========
var dsBuilder = new NpgsqlDataSourceBuilder(connString);
dsBuilder.MapEnum<TxType>("TX_TYPE");   // ★ PG ENUM 名稱必須與 DB 一致
var dataSource = dsBuilder.Build();

// 註冊唯一 DataSource
builder.Services.AddSingleton(dataSource);

// ========= Repos / Services =========
builder.Services.AddScoped<IListingRepo, ListingRepo>();

// UnitOfWork：每個 scope 建立一個交易上下文
builder.Services.AddScoped<IUnitOfWork>(sp =>
    new UnitOfWork(sp.GetRequiredService<NpgsqlDataSource>()));

builder.Services.AddScoped<Func<IUnitOfWork>>(sp => () => sp.GetRequiredService<IUnitOfWork>());

// 提供「工廠」，讓 Service 可以在同一個 UoW 下建立 Repo
builder.Services.AddScoped<Func<IUserRepo>>(sp => () =>
{
    var uow = (UnitOfWork)sp.GetRequiredService<IUnitOfWork>();
    return uow.CreateUserRepo();
});

// 應用服務
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<WalletService>();
builder.Services.AddScoped<IWalletQueryRepo, WalletQueryRepo>();


// ========= MVC / JSON / Swagger / CORS =========
builder.Services
    .AddControllers()
    .AddJsonOptions(opt =>
    {
        // 讓 Enum 輸出為字串而不是數字
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowFrontendDev", policy =>
	{
		policy
			.WithOrigins("https://localhost:7291")  // 前端網址（你開發用的）
			.AllowAnyHeader()
			.AllowAnyMethod()
			.AllowCredentials();
	});
});



// ========= Pipeline =========
var app = builder.Build();

app.UseCors("AllowFrontendDev");


app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
