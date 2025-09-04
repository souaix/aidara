
using Backend.Application.Listings;
using Backend.Application.Ports;
using Backend.Infrastructure.Persistence.Postgres;

using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// 讀取連線字串
var connString = builder.Configuration.GetConnectionString("Postgres");

// 建立 NpgsqlDataSource（比舊的 NpgsqlConnection 更好，支援連線池）
builder.Services.AddSingleton<NpgsqlDataSource>(_ =>
    NpgsqlDataSource.Create(connString));

// 註冊 Repository
builder.Services.AddScoped<IListingRepo, ListingRepo>();

// 註冊 UseCase
builder.Services.AddScoped<SearchListings>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var allowWeb = "_allowWeb";
builder.Services.AddCors(opts =>
{
    opts.AddPolicy(allowWeb, p =>
        p.WithOrigins("http://localhost:3000", "https://localhost:3000")
         .AllowAnyHeader()
         .AllowAnyMethod());
});

var app = builder.Build();
app.UseCors(allowWeb);


app.UseSwagger(); app.UseSwaggerUI();
app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
