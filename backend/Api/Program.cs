
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

// ========= UnitOfWork 設定 =========
#if DEBUG
Console.WriteLine("👉 DEBUG 模式：使用 FakeUnitOfWorkFactory");
builder.Services.AddScoped<IUnitOfWorkFactory, Backend.Infrastructure.Persistence.Mock.FakeUnitOfWorkFactory>();
#else
builder.Services.AddSingleton<IUnitOfWorkFactory>(
    _ => new Infrastructure.Persistence.Shared.PostgreSqlUnitOfWorkFactory(connString));
#endif

// ========= DataSource (供 Dapper 用) =========
builder.Services.AddSingleton(new NpgsqlDataSourceBuilder(connString).Build());

// ========= Scrutor 掃描 =========
var appAsm = Assembly.Load("Backend.Application");
var infraAsm = Assembly.Load("Backend.Infrastructure");

// ========= Repository 掃描 =========
#if DEBUG
Console.WriteLine("👉 DEBUG 模式：優先使用 Mock Repository，找不到再退回真實版本");

bool IsRepoClass(Type t) =>
	t.IsClass && !t.IsAbstract &&
	(t.Name.EndsWith("Repo", StringComparison.Ordinal) ||
	 t.Name.EndsWith("Repository", StringComparison.Ordinal));

bool IsMockNs(string? ns) =>
	ns is not null && ns.Contains(".Mock", StringComparison.Ordinal);

// 找出所有 Mock Repo
var mockRepoTypes = infraAsm.GetTypes().Where(t => IsRepoClass(t) && IsMockNs(t.Namespace)).ToList();
var mockInterfaceSet = new HashSet<Type>(mockRepoTypes.SelectMany(t => t.GetInterfaces()));

// 1️⃣ 先註冊所有真實 Repo（不管有沒有 Mock）
builder.Services.Scan(scan => scan
	.FromAssemblies(infraAsm)
	.AddClasses(c => c.Where(t =>
		IsRepoClass(t) &&
		t.Namespace != null &&
		t.Namespace.StartsWith("Backend.Infrastructure.Persistence.", StringComparison.Ordinal) &&
		!IsMockNs(t.Namespace)))
	.AsImplementedInterfaces()
	.WithScopedLifetime());

// 2️⃣ 再註冊 Mock Repo（只會覆蓋存在的介面）
if (mockRepoTypes.Count > 0)
{
	Console.WriteLine($"👉 偵測到 {mockRepoTypes.Count} 個 Mock Repositories，已覆蓋對應介面：");
	foreach (var t in mockRepoTypes)
	{
		Console.WriteLine($"   - {t.FullName}");
	}

	builder.Services.Scan(scan => scan
		.FromAssemblies(infraAsm)
		.AddClasses(c => c.Where(t => IsRepoClass(t) && IsMockNs(t.Namespace)))
		.AsImplementedInterfaces()
		.WithScopedLifetime());
}
else
{
	Console.WriteLine("⚠️ 未偵測到任何 Mock Repository，將使用真實 Repo。");
}

#else
Console.WriteLine("👉 RELEASE 模式：使用真實資料庫 Repositories");

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
		t.Namespace.StartsWith("Backend.Application.", StringComparison.Ordinal) &&
		t.Name.EndsWith("Service", StringComparison.Ordinal)))
	.AsImplementedInterfaces()
	.WithScopedLifetime());

builder.Services.Scan(scan => scan
	.FromAssemblies(appAsm)
	.AddClasses(c => c.Where(t =>
		t.Namespace is not null &&
		t.Namespace.StartsWith("Backend.Application.", StringComparison.Ordinal) &&
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
