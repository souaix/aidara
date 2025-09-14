using Backend.Application.ViewModels.Services;

public interface IBossServiceRepo
{
    Task ReplaceItemsAsync(Guid userId, List<Guid> itemIds, CancellationToken ct);
    Task ReplaceMethodsAsync(Guid userId, List<string> methods, CancellationToken ct);
    Task ReplaceAreasAsync(Guid userId, List<ServiceAreaDto> areas, CancellationToken ct);
    Task ReplaceAddressesAsync(Guid userId, List<ServiceAddressDto> addresses, CancellationToken ct);
}
