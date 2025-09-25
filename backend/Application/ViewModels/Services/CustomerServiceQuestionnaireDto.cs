public sealed record CustomerServiceQuestionnaireDto(
    Guid UserId,
    Guid ItemId,
    int? PriceMin,
    int? PriceMax,
    List<string> Methods
);
