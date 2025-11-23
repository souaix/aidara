using Backend.Application.ViewModels.Services;

namespace Backend.Application.Ports; // 新增

public interface IBossServiceMethodRepo
{
    Task<List<BossServiceMethodVm>> GetBossMethodsAsync(Guid userId, CancellationToken ct);
    Task UpdateBossMethodsAsync(Guid userId, List<string> methods, CancellationToken ct);
}
