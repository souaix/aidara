using Backend.Application.Ports;
using Backend.Application.ViewModels.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BossServicesController : ControllerBase
    {
        private readonly IServiceRepo _serviceRepo;
        private readonly IUserServiceRepo _userServiceRepo;

        public BossServicesController(IServiceRepo serviceRepo, IUserServiceRepo userServiceRepo)
        {
            _serviceRepo = serviceRepo;
            _userServiceRepo = userServiceRepo;
        }

        /// <summary>
        /// 取得所有服務分類 (含中分類與服務項目)
        /// </summary>
        [HttpGet("categories")]
        public async Task<ActionResult<List<ServiceCategoryVm>>> GetCategories(CancellationToken ct)
        {
            var categories = await _serviceRepo.GetAllCategoriesAsync(ct);
            return Ok(categories);
        }

        /// <summary>
        /// 取得使用者已勾選的服務項目
        /// </summary>
        [HttpGet("user-services/{userId:guid}")]
        public async Task<ActionResult<List<UserServiceItemVm>>> GetUserServices(Guid userId, CancellationToken ct)
        {
            var items = await _userServiceRepo.GetUserServicesAsync(userId, ct);
            return Ok(items);
        }

        /// <summary>
        /// 更新使用者的服務項目 (覆蓋舊的)
        /// </summary>
        [HttpPost("user-services/{userId:guid}")]
        public async Task<IActionResult> UpdateUserServices(Guid userId, [FromBody] List<Guid> itemIds, CancellationToken ct)
        {
            await _userServiceRepo.UpdateUserServicesAsync(userId, itemIds, ct);
            return NoContent();
        }
    }
}
