using Backend.Application.Ports;
using Backend.Application.Services;
using Backend.Application.ViewModels.Services;
using Backend.Infrastructure.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BossInfoController : ControllerBase
    {
        private readonly BossInfoQuestionnaireService _svc;
        private readonly IUnitOfWork _uow;

        public BossInfoController(IUnitOfWork uow, BossInfoQuestionnaireService svc)
        {
            _uow = uow;
            _svc = svc;
        }

        /// <summary>
        /// 將老闆問卷資料送出並儲存
        /// </summary>
        [HttpPost("BossInfoQuestionSubmit")]
        public async Task<IActionResult> BossInfoQuestionSubmit([FromBody] BossInfoQuestionnaireDto dto, CancellationToken ct)
        {
            await _svc.SubmitAsync(dto, ct);
            return NoContent();

        }




        /// <summary>
        /// 取得使用者已勾選的服務項目
        /// </summary>
        [HttpGet("user-services/{userId:guid}")]
        public async Task<ActionResult<List<UserServiceItemVm>>> GetUserServices(Guid userId, CancellationToken ct)
        {
            var repo = _uow.CreateUserServiceRepo();
            var items = await repo.GetUserServicesAsync(userId, ct);
            return Ok(items);
        }

        /// <summary>
        /// 更新使用者的服務項目 (覆蓋舊的)
        /// </summary>
        [HttpPost("user-services/{userId:guid}")]
        public async Task<IActionResult> UpdateUserServices(Guid userId, [FromBody] List<Guid> itemIds, CancellationToken ct)
        {
            await _uow.BeginAsync(ct);
            try
            {
                var repo = _uow.CreateUserServiceRepo();
                await repo.UpdateUserServicesAsync(userId, itemIds, ct);

                await _uow.CommitAsync(ct);
                return NoContent();
            }
            catch
            {
                await _uow.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// 取得某服務項目在各縣市區的店家數
        /// </summary>
        [HttpGet("stats/{itemId:guid}")]
        public async Task<ActionResult<List<ServiceStatVm>>> GetServiceStats(Guid itemId, CancellationToken ct)
        {
            var repo = _uow.CreateServiceStatRepo();
            var stats = await repo.GetServiceStatsAsync(itemId, ct);
            return Ok(stats);
        }

        /// <summary>
        /// 取得指定地區的店家清單
        /// </summary>
        [HttpGet("stores/{itemId:guid}")]
        public async Task<ActionResult<List<StoreVm>>> GetStoresByRegion(
            Guid itemId,
            [FromQuery] string cityId,
            [FromQuery] string? districtId,
            CancellationToken ct)
        {
            var repo = _uow.CreateBossStoreRepo();
            var stores = await repo.GetStoresByRegionAsync(itemId, cityId, districtId, ct);
            return Ok(stores);
        }

        /// <summary>
        /// 取得單一商家的詳細資訊 (問卷結果 + 自定義 Quill 內容)
        /// </summary>
        [HttpGet("store/{userId:guid}")]
        public async Task<ActionResult<StoreDetailVm>> GetStoreDetail(Guid userId, CancellationToken ct)
        {
            var repo = _uow.CreateBossStoreRepo();
            var detail = await repo.GetStoreDetailAsync(userId, ct);
            if (detail is null) return NotFound();
            return Ok(detail);
        }
    }
}
