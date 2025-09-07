using Backend.Application.Listings;
using Backend.Application.Ports;
using Backend.Application.Users;
using Backend.Infrastructure.Persistence.Postgres;
using Npgsql;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

// 讀取連線字串
var connString = builder.Configuration.GetConnectionString("Postgres");

// 建立 NpgsqlDataSource（連線池）
builder.Services.AddSingleton<NpgsqlDataSource>(_ => NpgsqlDataSource.Create(connString));

// Listings
builder.Services.AddScoped<IListingRepo, ListingRepo>();
builder.Services.AddScoped<SearchListings>();

// === Users with UoW（重點）===

// ❌ 移除這行（不要直接註冊 Repo 實作）
// builder.Services.AddScoped<IUserRepo, UserRepo>();

// UnitOfWork：每請求一個交易上下文
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// 提供「介面工廠」：在同一 UoW（同一連線+交易）下產生 IUserRepo
builder.Services.AddScoped<Func<IUserRepo>>(sp =>
{
    return () =>
    {
        var uow = (UnitOfWork)sp.GetRequiredService<IUnitOfWork>();
        return uow.CreateUserRepo();
    };
});

// ❌ 不需要，也請移除：Func<UserRepo>
// builder.Services.AddScoped<Func<UserRepo>>(sp => () => sp.GetRequiredService<UserRepo>());

// 用例
builder.Services.AddScoped<AuthService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var allowWeb = "_allowWeb";

builder.Services
    .AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddCors(opts =>
{
    opts.AddPolicy(allowWeb, p =>
        p.WithOrigins("http://localhost:3000", "https://localhost:3000")
         .AllowAnyHeader()
         .AllowAnyMethod());
});

var app = builder.Build();
app.UseCors(allowWeb);

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
