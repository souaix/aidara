
using Backend.Domain.Geo;
using Backend.Domain.Listings;

namespace Backend.Application.Ports;

public interface IListingRepo
{
    Task<(IReadOnlyList<Listing> Items, int Total)> SearchAsync(
        BBox bbox,
        decimal? minUnit,
        decimal? maxUnit,
        int? minRooms,
        int? maxRooms,
        int page,
        int pageSize,
        CancellationToken ct);
}
