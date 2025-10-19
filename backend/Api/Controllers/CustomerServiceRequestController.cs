// Backend.Api/Controllers/CustomerServiceRequestController.cs
using Backend.Application.Ports;
using Backend.Application.ViewModels.Services;
using Backend.Application.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerServiceRequestController : ControllerBase
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly ICustomerServiceRequestRepo _repo;

        public CustomerServiceRequestController(
            IUnitOfWorkFactory uowFactory,
            ICustomerServiceRequestRepo repo)
        {
            _uowFactory = uowFactory;
            _repo = repo;
        }

        /// <summary>
        /// 顧客服務請求提交（新建一筆）
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] CustomerServiceRequestDto dto, CancellationToken ct)
        {
            // 基本檢核
            if (dto.UserId == Guid.Empty || dto.ItemId == Guid.Empty)
                return BadRequest("UserId / ItemId 不可為空");

            if (dto.PriceMin is not null && dto.PriceMax is not null && dto.PriceMin > dto.PriceMax)
                return BadRequest("price_min 不可大於 price_max");

            await using var uow = await _uowFactory.BeginAsync(withTransaction: true, ct);

            try
            {
                await _repo.InsertAsync(uow.Connection, uow.Transaction, dto, ct);
                await uow.CommitAsync(ct);
                return NoContent();
            }
            catch (Exception ex)
            {
                await uow.RollbackAsync(ct);
                Console.WriteLine($"❗CustomerServiceRequest Submit Failed: {ex.Message}");
                throw;
            }
        }
    }
}
