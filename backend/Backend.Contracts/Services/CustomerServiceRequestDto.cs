public sealed record CustomerServiceRequestDto(
    Guid UserId,
    Guid ItemId,
    int? PriceMin,
    int? PriceMax,
    List<string> Methods
);
