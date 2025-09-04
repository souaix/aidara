namespace Backend.Domain.Listings;

public sealed class Listing
{
    public long Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public decimal? PriceUnit { get; init; }
    public int? Rooms { get; init; }
    public double Lon { get; init; }
    public double Lat { get; init; }
}
