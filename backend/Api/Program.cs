
using Backend.Application.Ports;
using Backend.Application.Services.Boss;
using Backend.Application.Services.Users;
using Backend.Application.Shared;
using Backend.Infrastructure.Persistence.Postgres;
using Backend.Infrastructure.Persistence.Mock;
using Infrastructure.Persistence.Shared;   // 放 PostgreSqlUnitOfWorkFactory
using Npgsql;
using System.Reflection;
using System.Text.Json.Serialization;

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

// ========= Connection =========
var connString = builder.Configuration.GetConnectionString("Postgres");

// PostgreSQL UnitOfWork Factory（跨資料庫統一介面）
builder.Services.AddSingleton<IUnitOfWorkFactory>(
	_ => new PostgreSqlUnitOfWorkFactory(connString));

// 若仍需要 DataSource 供 Dapper / Repo 使用，可保留
builder.Services.AddSingleton(new NpgsqlDataSourceBuilder(connString).Build());

// ========= Scrutor 掃描 =========
var appAsm = Assembly.Load("Backend.Application");
var infraAsm = Assembly.Load("Backend.Infrastructure");



// ===== Repository 掃描 =====
#if DEBUG
bool IsRepoClass(Type t) =>
	t.IsClass && !t.IsAbstract &&
	(t.Name.EndsWith("Repo", StringComparison.Ordinal) ||
	 t.Name.EndsWith("Repository", StringComparison.Ordinal));

bool IsMockNs(string? ns) =>
	ns is not null &&
	(ns.Contains(".Mock.", StringComparison.Ordinal) || ns.EndsWith(".Mock", StringComparison.Ordinal));

var mockRepoTypes = infraAsm.GetTypes().Where(t => IsRepoClass(t) && IsMockNs(t.Namespace));
var mockInterfaceSet = new HashSet<Type>(mockRepoTypes.SelectMany(t => t.GetInterfaces()));

builder.Services.Scan(scan => scan
    .FromAssemblies(infraAsm)
    .AddClasses(c => c.Where(t =>
        IsRepoClass(t) &&
        t.Namespace != null &&
        t.Namespace.StartsWith("Backend.Infrastructure.Persistence.", StringComparison.Ordinal) &&
        !IsMockNs(t.Namespace) &&
        !t.GetInterfaces().Any(i => mockInterfaceSet != null && mockInterfaceSet.Contains(i))
    ))
    .AsImplementedInterfaces()
    .WithScopedLifetime());


builder.Services.Scan(scan => scan
	.FromAssemblies(infraAsm)
	.AddClasses(c => c.Where(t => IsRepoClass(t) && IsMockNs(t.Namespace)))
	.AsImplementedInterfaces()
	.WithScopedLifetime());
#else
builder.Services.Scan(scan => scan
    .FromAssemblies(infraAsm)
    .AddClasses(c => c.Where(t =>
        t.IsClass && !t.IsAbstract &&
        (t.Name.EndsWith("Repo", StringComparison.Ordinal) ||
         t.Name.EndsWith("Repository", StringComparison.Ordinal)) &&
        t.Namespace is not null &&
        t.Namespace.StartsWith("Backend.Infrastructure.Persistence.", StringComparison.Ordinal) &&
        !t.Namespace.Contains(".Mock", StringComparison.Ordinal)))
    .AsImplementedInterfaces()
    .WithScopedLifetime());
#endif

// ===== Application Services =====
builder.Services.Scan(scan => scan
	.FromAssemblies(appAsm)
	.AddClasses(c => c.Where(t =>
		t.Namespace is not null &&
		t.Namespace.StartsWith("Application.", StringComparison.Ordinal) &&
		t.Name.EndsWith("Service", StringComparison.Ordinal)))
	.AsImplementedInterfaces()
	.WithScopedLifetime());

builder.Services.Scan(scan => scan
	.FromAssemblies(appAsm)
	.AddClasses(c => c.Where(t =>
		t.Namespace is not null &&
		t.Namespace.StartsWith("Application.", StringComparison.Ordinal) &&
		t.Name.EndsWith("Service", StringComparison.Ordinal)))
	.AsSelf()
	.WithScopedLifetime());

// ========= MVC / JSON / Swagger / CORS =========
builder.Services
	.AddControllers()
	.AddJsonOptions(opt =>
	{
		opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
	});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowFrontendDev", policy =>
	{
		policy
			.WithOrigins("https://localhost:7291")
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
