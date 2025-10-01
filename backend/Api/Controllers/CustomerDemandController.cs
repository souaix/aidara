using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CustomerDemandController : ControllerBase
{
    private readonly ICustomerDemandRepo _repo;
    public CustomerDemandController(ICustomerDemandRepo repo) => _repo = repo;

    // GET /api/CustomerDemand/cities
    [HttpGet("cities")]
    public async Task<IActionResult> GetCities(CancellationToken ct)
    {
        var cities = await _repo.GetCityCountsAsync(ct);
        return Ok(cities);
    }

    // GET /api/CustomerDemand/list/{city}
    [HttpGet("list/{city}")]
    public async Task<IActionResult> GetByCity(string city, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(city)) return BadRequest("city is required");
        var demands = await _repo.GetDemandsByCityAsync(city, ct);
        return Ok(demands);
    }
}
