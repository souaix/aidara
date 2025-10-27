using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Backend.Application.Ports
{
    public interface IUserActiveModeRepo
    {
        /// <summary>
        /// 取得使用者目前的操作模式（CUSTOMER / BOSS / GOLDENBOSS 等）
        /// </summary>
        Task<string?> GetActiveModeAsync(IDbConnection conn, IDbTransaction? tx,Guid userId, CancellationToken ct);

        /// <summary>
        /// 設定使用者當前操作模式（若無紀錄則新增）
        /// </summary>
        Task SetActiveModeAsync(IDbConnection conn, IDbTransaction? tx,Guid userId, string roleId, CancellationToken ct);
    }
}
