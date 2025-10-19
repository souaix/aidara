using Backend.Application.ViewModels.Services;

public interface IBossServiceMethodRepo
{
    Task<List<BossServiceMethodVm>> GetBossMethodsAsync(Guid userId, CancellationToken ct);
    Task UpdateBossMethodsAsync(Guid userId, List<string> methods, CancellationToken ct);
}
