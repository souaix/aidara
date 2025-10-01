public sealed record CustomerServiceQuestionnaireDto(
    Guid ItemId,
    int? PriceMin,
    int? PriceMax,
    List<string> Methods,
    string? City
);
