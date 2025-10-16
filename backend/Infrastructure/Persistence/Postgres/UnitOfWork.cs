using Backend.Application.Ports;
using Backend.Infrastructure.Persistence.Postgres;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Backend.Application.Contracts.Users;
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly NpgsqlDataSource _ds;
    private readonly IServiceProvider _sp;
    private NpgsqlConnection? _conn;
    private NpgsqlTransaction? _tx;

    public UnitOfWork(NpgsqlDataSource ds, IServiceProvider sp)
    {
        _ds = ds;
        _sp = sp;
    }

    // Transaction：只有寫入時才會呼叫
    public async Task BeginAsync(CancellationToken ct)
    {
        _conn = (NpgsqlConnection)await _ds.OpenConnectionAsync(ct);
        _tx = await _conn.BeginTransactionAsync(ct);
    }

    public Task CommitAsync(CancellationToken ct) => _tx!.CommitAsync(ct);
    public Task RollbackAsync() => _tx!.RollbackAsync();

    public async ValueTask DisposeAsync()
    {
        await (_tx?.DisposeAsync() ?? ValueTask.CompletedTask);
        await (_conn?.DisposeAsync() ?? ValueTask.CompletedTask);
    }


    // ========== Transaction 內部使用的 Repo ==========
    public IUserRoleRepo CreateUserRepo() => new UserRepo(_conn!, _tx);
    // ✅ 新增 UserRole / RoleBasis Repo 實例化方法
    public IRoleBasisRepo CreateRoleBasisRepo() => new RoleBasisRepo(_conn!, _tx);


    //以下未確認


    public IWalletRepo CreateWalletRepo() => new WalletRepo(_conn!, _tx);

    //[modify] 20251008
    public IBossServiceRepo CreateBossInfoRepo() => new BossInfoRepo(_conn!, _tx);

    public ICustomerServiceRequestRepo CreateCustomerServiceRequestRepo() => new CustomerServiceRequestRepo(_conn!, _tx);

    // ========== 只讀或可由 DI 控制 Mock/Real 的 Repo ==========
    public IBossStoreRepo CreateBossStoreRepo() => _sp.GetRequiredService<IBossStoreRepo>();
    public IServiceRepo CreateServiceRepo() => _sp.GetRequiredService<IServiceRepo>();
    public IUserServiceRepo CreateUserServiceRepo() => _sp.GetRequiredService<IUserServiceRepo>();
    public IServiceStatRepo CreateServiceStatRepo() => _sp.GetRequiredService<IServiceStatRepo>();

    public ICustomerServiceQuestionnaireRepo CreateCustomerServiceQuestionnaireRepo()=> new CustomerServiceQuestionnaireRepo(_conn!, _tx);



}

