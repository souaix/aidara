// Backend.Api/Controllers/LocationController.cs
using Backend.Application.Ports;
using Backend.Application.Shared;
using Backend.Application.ViewModels;
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

        [HttpPost("districtsbycities")]
        public async Task<ActionResult<List<LocationDistrictVm>>> GetDistrictsByCities([FromBody] List<int> cityIds, CancellationToken ct)
        {
            await using var uow = await _uowFactory.BeginAsync(withTransaction: false, ct);
            if (cityIds == null || cityIds.Count == 0)
                return BadRequest("未提供城市 ID");

            var districts = await _repo.GetDistrictsByCitiesAsync(uow.Connection, uow.Transaction, cityIds, ct);
            return Ok(districts);
        }


        /// <summary>
        /// 取得指定行政區底下的所有郵遞區號
        /// </summary>
        [HttpGet("postal/{districtId:int}")]
        public async Task<ActionResult<List<LocationPostalVm>>> GetPostal(int districtId, CancellationToken ct)
        {
            await using var uow = await _uowFactory.BeginAsync(withTransaction: false, ct);
            var postals = await _repo.GetPostalByDistrictAsync(uow.Connection, uow.Transaction, districtId, ct);

            if (postals is null || postals.Count == 0)
                return NotFound("查無該行政區的郵遞區號資料");

            return Ok(postals);
        }

        /// <summary>
        /// 取得指定行政區底下的所有郵遞區號
        /// </summary>
        [HttpPost("postals")]
        public async Task<ActionResult<List<LocationPostalVm>>> GetPostals([FromBody] List<int> districtIds, CancellationToken ct)
        {
            await using var uow = await _uowFactory.BeginAsync(withTransaction: false, ct);
            if (districtIds is null || districtIds.Count == 0)
                return BadRequest("未提供行政區 ID");

            var postals = await _repo.GetPostalsByDistrictsAsync(uow.Connection, uow.Transaction, districtIds, ct);
            return Ok(postals);
        }

    }
}
