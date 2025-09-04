using Microsoft.AspNetCore.Mvc;
using Backend.Application.Listings;
using Backend.Domain.Geo;
using Backend.Api.Contracts.Listings;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ListingsController : ControllerBase
{
    [HttpGet("search")]
    public async Task<ActionResult<SearchListingsResponse>> Search(
        [FromQuery] SearchListingsRequest req,
        [FromServices] SearchListings useCase,
        CancellationToken ct)
    {
        var bbox = new BBox(req.MinLon, req.MinLat, req.MaxLon, req.MaxLat);

        var (items, total) = await useCase.HandleAsync(
            bbox,
            req.MinUnit, req.MaxUnit, req.MinRooms, req.MaxRooms,
            req.Page <= 0 ? 1 : req.Page,
            req.PageSize <= 0 ? 50 : req.PageSize,
            ct);

        var dto = new SearchListingsResponse(
            total,
            items.Select(x => new ListingItemDto(
                x.Id, x.Title, x.PriceUnit, x.Rooms, x.Lon, x.Lat)));

        return Ok(dto);
    }
}
