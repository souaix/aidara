namespace Backend.Application.Ports;

public interface IUnitOfWork : IAsyncDisposable
{
    Task BeginAsync(CancellationToken ct);
    Task CommitAsync(CancellationToken ct);
    Task RollbackAsync();

    // 會員 / 使用者
    IUserRepo CreateUserRepo();
    IRoleBasisRepo CreateRoleBasisRepo();


    // 錢包
    IWalletRepo CreateWalletRepo();

    // 老闆服務設定
    IBossServiceRepo CreateBossInfoRepo();

    // 老闆商店（清單、詳細資訊）
    IBossStoreRepo CreateBossStoreRepo();

    // 服務分類 / 項目
    IServiceRepo CreateServiceRepo();

    // 使用者選過的服務項目
    IUserServiceRepo CreateUserServiceRepo();

    // 統計 (各縣市商家數)
    IServiceStatRepo CreateServiceStatRepo();

    ICustomerServiceRequestRepo CreateCustomerServiceRequestRepo();

    ICustomerServiceQuestionnaireRepo CreateCustomerServiceQuestionnaireRepo();

}
