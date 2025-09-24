using Backend.Application.Ports;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerServiceRequestController : ControllerBase
    {
        private readonly IUnitOfWork _uow;

        public CustomerServiceRequestController(IUnitOfWork uow) => _uow = uow;

        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] CustomerServiceRequestDto dto, CancellationToken ct)
        {
            // 基本檢核（可依需要擴充）
            if (dto.UserId == Guid.Empty || dto.ItemId == Guid.Empty)
                return BadRequest("UserId / ItemId 不可為空");

            if (dto.PriceMin is not null && dto.PriceMax is not null && dto.PriceMin > dto.PriceMax)
                return BadRequest("price_min 不可大於 price_max");

            await _uow.BeginAsync(ct);
            try
            {
                var repo = _uow.CreateCustomerServiceRequestRepo();
                await repo.InsertAsync(dto, ct);
                await _uow.CommitAsync(ct);
                return NoContent();
            }
            catch
            {
                await _uow.RollbackAsync();
                throw;
            }
        }
    }
}
