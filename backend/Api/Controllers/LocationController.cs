// Backend.Api/Controllers/LocationController.cs
using Backend.Application.Ports;
using Backend.Application.Shared;
using Backend.Application.ViewModels.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationController : ControllerBase
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly ILocationRepo _repo;

        public LocationController(IUnitOfWorkFactory uowFactory, ILocationRepo repo)
        {
            _uowFactory = uowFactory;
            _repo = repo;
        }

        [HttpGet("cities")]
        public async Task<ActionResult<List<LocationCityVm>>> GetCities(CancellationToken ct)
        {
            await using var uow = await _uowFactory.BeginAsync(withTransaction: false, ct);
            var cities = await _repo.GetCitiesAsync(uow.Connection, uow.Transaction, ct);
            return Ok(cities);
        }

        [HttpGet("districts/{cityId:int}")]
        public async Task<ActionResult<List<LocationDistrictVm>>> GetDistricts(int cityId, CancellationToken ct)
        {
            await using var uow = await _uowFactory.BeginAsync(withTransaction: false, ct);
            var districts = await _repo.GetDistrictsByCityAsync(uow.Connection, uow.Transaction, cityId, ct);
            return Ok(districts);
        }
    }
}
