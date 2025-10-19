// Backend.Api/Controllers/BossInfoController.cs
using Backend.Application.Ports;

using Backend.Application.Services.Boss;
using Backend.Application.Shared;
using Backend.Application.ViewModels;
using Backend.Application.ViewModels.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BossInfoController : ControllerBase
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IBossInfoRepo _bossInfoRepo;
        private readonly IBossServiceStatRepo _serviceStatRepo;
        private readonly IBossStoreRepo _bossStoreRepo;
     
        private readonly BossInfoQuestionnaireService _svc;

        public BossInfoController(
            IUnitOfWorkFactory uowFactory,
            IBossInfoRepo bossInfoRepo,
            IBossServiceStatRepo serviceStatRepo,
            IBossStoreRepo bossStoreRepo,
            BossInfoQuestionnaireService svc)
        {
            _uowFactory = uowFactory;
            _bossInfoRepo = bossInfoRepo;
            _serviceStatRepo = serviceStatRepo;
            _bossStoreRepo = bossStoreRepo;
       
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
        /// 取得某服務項目在各縣市區的店家數
        /// </summary>
        [HttpGet("stats/{itemId:guid}")]
        public async Task<ActionResult<List<ServiceStatVm>>> GetServiceStats(Guid itemId, CancellationToken ct)
        {
            await using var uow = await _uowFactory.BeginAsync(withTransaction: false, ct);
            var stats = await _serviceStatRepo.GetServiceStatsAsync(uow.Connection, uow.Transaction, itemId, ct);
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
            await using var uow = await _uowFactory.BeginAsync(withTransaction: false, ct);
            var stores = await _bossStoreRepo.GetStoresByRegionAsync(uow.Connection, uow.Transaction, itemId, cityId, districtId, ct);
            return Ok(stores);
        }

        /// <summary>
        /// 取得單一商家的詳細資訊 (問卷結果 + 自定義 Quill 內容)
        /// </summary>
        [HttpGet("store/{userId:guid}")]
        public async Task<ActionResult<StoreDetailVm>> GetStoreDetail(Guid userId, CancellationToken ct)
        {
            await using var uow = await _uowFactory.BeginAsync(withTransaction: false, ct);
            var detail = await _bossStoreRepo.GetStoreDetailAsync(uow.Connection, uow.Transaction, userId, ct);
            if (detail is null)
                return NotFound();

            return Ok(detail);
        }
    }
}
