using Backend.Application.Ports;
using Backend.Application.ViewModels.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationController : ControllerBase
    {
        private readonly ILocationRepo _repo;

        public LocationController(ILocationRepo repo)
        {
            _repo = repo;
        }

        [HttpGet("cities")]
        public async Task<ActionResult<List<LocationCityVm>>> GetCities(CancellationToken ct)
        {
            var cities = await _repo.GetCitiesAsync(ct);
            return Ok(cities);
        }

        [HttpGet("districts/{cityId:int}")]
        public async Task<ActionResult<List<LocationDistrictVm>>> GetDistricts(int cityId, CancellationToken ct)
        {
            var districts = await _repo.GetDistrictsByCityAsync(cityId, ct);
            return Ok(districts);
        }
    }
}
