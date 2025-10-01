public interface ICustomerDemandRepo
{
    Task<IReadOnlyList<CityCountVm>> GetCityCountsAsync(CancellationToken ct);
    Task<IReadOnlyList<DemandVm>> GetDemandsByCityAsync(string city, CancellationToken ct);
}

// 回傳用 VM
public sealed record CityCountVm(string City, int Count);

public sealed record DemandVm(
    Guid RequestId,
    string City,
    string ItemName,
    int? PriceMin,
    int? PriceMax,
    IReadOnlyList<string> Methods,
    DateTime CreatedAt
);
