using System.Data;

namespace Backend.Application.Shared
{
	/// <summary>
	/// 通用的 Unit of Work 介面，封裝連線與交易控制。
	/// </summary>
	public interface IUnitOfWork : IAsyncDisposable
	{
		/// <summary>取得目前使用的資料庫連線。</summary>
		IDbConnection Connection { get; }

		/// <summary>取得目前的交易物件（可為 null 表示未使用交易）。</summary>
		IDbTransaction? Transaction { get; }

		/// <summary>提交交易。</summary>
		Task CommitAsync(CancellationToken ct = default);

		/// <summary>回滾交易。</summary>
		Task RollbackAsync(CancellationToken ct = default);
	}

	/// <summary>
	/// 用於建立新的 UnitOfWork 實例（例如 PostgreSQL、Oracle）。
	/// </summary>
	public interface IUnitOfWorkFactory
	{
		/// <summary>
		/// 建立新的 UnitOfWork。
		/// </summary>
		/// <param name="withTransaction">是否啟用交易（預設 true）</param>
		/// <param name="ct">取消權杖</param>
		Task<IUnitOfWork> BeginAsync(bool withTransaction = true, CancellationToken ct = default);
	}
}

//namespace Backend.Application.Ports;

//public interface IUnitOfWork : IAsyncDisposable
//{
//    Task BeginAsync(CancellationToken ct);
//    Task CommitAsync(CancellationToken ct);
//    Task RollbackAsync();

//    // 會員 / 使用者
//    IUserRoleRepo CreateUserRepo();
//    IRoleBasisRepo CreateRoleBasisRepo();


//    // 錢包
//    IWalletRepo CreateWalletRepo();

//    // 老闆服務設定
//    IBossServiceRepo CreateBossInfoRepo();

//    // 老闆商店（清單、詳細資訊）
//    IBossStoreRepo CreateBossStoreRepo();

//    // 服務分類 / 項目
//    IServiceRepo CreateServiceRepo();

//    // 使用者選過的服務項目
//    IUserServiceRepo CreateUserServiceRepo();

//    // 統計 (各縣市商家數)
//    IServiceStatRepo CreateServiceStatRepo();

//    ICustomerServiceRequestRepo CreateCustomerServiceRequestRepo();

//    ICustomerServiceQuestionnaireRepo CreateCustomerServiceQuestionnaireRepo();

//}
