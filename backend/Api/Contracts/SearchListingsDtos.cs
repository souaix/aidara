namespace Backend.Api.Contracts.Listings;

public sealed record SearchListingsRequest(
    double MinLon,
    double MinLat,
    double MaxLon,
    double MaxLat,
    decimal? MinUnit,
    decimal? MaxUnit,
    int? MinRooms,
    int? MaxRooms,
    int Page = 1,
    int PageSize = 50);

public sealed record ListingItemDto(
    long Id,
    string Title,
    decimal? PriceUnit,
    int? Rooms,
    double Lon,
    double Lat);

public sealed record SearchListingsResponse(
    int Total,
    IEnumerable<ListingItemDto> Items);
