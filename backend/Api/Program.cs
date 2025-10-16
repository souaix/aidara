using Backend.Application.Listings;
using Backend.Application.Ports;
using Backend.Application.Services.Boss;
using Backend.Application.Services.Users;
using Backend.Application.Wallet;
using Backend.Domain.Wallet;
using Backend.Infrastructure.Persistence.Postgres;

using Infrastructure.Persistence.Mock;
using Npgsql;
using System.Text.Json.Serialization;

// 在建構 Host 前手動設定環境
#if DEBUG
Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
#else
Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
#endif


var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

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
    new UnitOfWork(
        sp.GetRequiredService<NpgsqlDataSource>(),
        sp  // 把 ServiceProvider 傳進去
    ));


builder.Services.AddScoped<Func<IUnitOfWork>>(sp => () => sp.GetRequiredService<IUnitOfWork>());

// 提供「工廠」，讓 Service 可以在同一個 UoW 下建立 Repo
builder.Services.AddScoped<Func<IUserRoleRepo>>(sp => () =>
{
    var uow = (UnitOfWork)sp.GetRequiredService<IUnitOfWork>();
    return uow.CreateUserRepo();
});

// 應用服務
builder.Services.AddScoped<AuthService>();

//以下未確認

builder.Services.AddScoped<WalletService>();
builder.Services.AddScoped<IWalletQueryRepo, WalletQueryRepo>();


builder.Services.AddScoped<ILocationRepo, LocationRepo>();

builder.Services.AddScoped<BossInfoQuestionnaireService>();
builder.Services.AddScoped<CustomerServiceQuestionnaireService>();

builder.Services.AddScoped<IServiceStatService, ServiceStatService>();

if (builder.Environment.IsDevelopment())
{	
	builder.Services.AddSingleton<IServiceStatRepo, MockServiceStatRepo>();
	builder.Services.AddScoped<IUserServiceRepo, MockUserServiceRepo>();
	builder.Services.AddScoped<IServiceRepo, MockServiceRepo>();
    builder.Services.AddScoped<IBossStoreRepo, MockBossStoreRepo>();
    builder.Services.AddSingleton<ICustomerServiceRequestRepo, MockCustomerServiceRequestRepo>();

}
else
{
	builder.Services.AddScoped<IServiceStatRepo, ServiceStatRepo>();
	builder.Services.AddScoped<IUserServiceRepo, UserServiceRepo>();
	builder.Services.AddScoped<IServiceRepo, ServiceRepo>();
    builder.Services.AddScoped<IBossStoreRepo, BossStoreRepo>();
    builder.Services.AddSingleton<ICustomerServiceRequestRepo, CustomerServiceRequestRepo>();
}


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
app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("AllowFrontendDev");


app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
