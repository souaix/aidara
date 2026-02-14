// Backend.Api/Controllers/BossInfoController.cs
using Backend.Application.Ports;

using Backend.Application.Services.Boss;
using Backend.Application.Shared;
using Backend.Application.ViewModels;
using Backend.Application.ViewModels.Boss;
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
        //[HttpGet("store/{userId:guid}")]
        //public async Task<ActionResult<StoreDetailVm>> GetStoreDetail(Guid userId, CancellationToken ct)
        //{
        //    await using var uow = await _uowFactory.BeginAsync(withTransaction: false, ct);
        //    var detail = await _bossStoreRepo.GetStoreDetailAsync(uow.Connection, uow.Transaction, userId, ct);
        //    if (detail is null)
        //        return NotFound();

        //    return Ok(detail);
        //}

        /// <summary>
        /// 取得小老闆所有上架的服務（含方法 / 範圍 / 地址）
        /// </summary>
        [HttpGet("store/{userId:guid}")]
        public async Task<ActionResult<List<BossServiceFullVm>>> GetBossServices(Guid userId, CancellationToken ct)
        {
            await using var uow = await _uowFactory.BeginAsync(withTransaction: false, ct);
            var services = await _bossStoreRepo.GetBossServicesAsync(uow.Connection, uow.Transaction, userId, ct);
            if (services == null || !services.Any())
                return Ok(Array.Empty<BossServiceFullVm>()); // 統一回傳空陣列

            return Ok(services);
        }

        /// <summary>
        /// 取代小老闆的服務區域（完整覆蓋）
        /// </summary>
        [HttpPost("store/upsertServiceAreas/{userId:guid}")]
        public async Task<IActionResult> UpsertServiceAreas(Guid userId, [FromBody] List<ServiceAreaDto> areas, CancellationToken ct)
        {

            await using var uow = await _uowFactory.BeginAsync(withTransaction: true, ct);

            await _bossInfoRepo.ReplaceAreasAsync(uow.Connection, uow.Transaction, userId, areas, ct);

            await uow.CommitAsync();   // ⬅ 一定要有這行

            return NoContent();

        }

        /// <summary>
        /// 取代小老闆的服務據點（完整覆蓋）
        /// </summary>
        [HttpPost("store/upsertServiceAddresses/{userId:guid}")]
        public async Task<IActionResult> UpsertServiceAddresses(Guid userId, [FromBody] List<ServiceAddressDto> addresses, CancellationToken ct)
        {

            await using var uow = await _uowFactory.BeginAsync(withTransaction: true, ct);

            await _bossInfoRepo.ReplaceAddressesAsync(uow.Connection, uow.Transaction, userId, addresses, ct);

            await uow.CommitAsync();   // ⬅ 一定要有這行

            return NoContent();

        }

        [HttpPost("store/upsert")]
        public async Task<IActionResult> UpsertService([FromBody] BossServiceUpsertDto dto, CancellationToken ct)
        {

            await using var uow = await _uowFactory.BeginAsync(withTransaction: true, ct);

            await _bossInfoRepo.UpsertBossServiceAsync(uow.Connection, uow.Transaction, dto, ct);

            await uow.CommitAsync();   // ⬅ 一定要有這行

            return NoContent();

        }

        [HttpPost("store/upsertServierItem")]
        public async Task<IActionResult> UpsertServiceItem([FromBody] BossServiceUpsertDto dto, CancellationToken ct)
        {
            await using var uow = await _uowFactory.BeginAsync(withTransaction: true, ct);

            var item = new ItemPriceRangeDto
            {
                ItemId = dto.ItemId,
                MinPrice = dto.MinPrice,
                MaxPrice = dto.MaxPrice,
                IsActive = dto.IsActive
            };

            await _bossInfoRepo.UpsertItemAsync(uow.Connection, uow.Transaction, dto.UserId, item, ct);

            await uow.CommitAsync();   // ⬅ 一定要有這行

            return NoContent();

        }

        [HttpPost("store/deleteServierItem")]
        public async Task<IActionResult> deleteServiceItem([FromBody] DeleteServiceItemDto dto, CancellationToken ct)
        {
            await using var uow = await _uowFactory.BeginAsync(withTransaction: true, ct);

            await _bossInfoRepo.DeleteItemAsync(uow.Connection, uow.Transaction, dto.UserId, dto.ItemId, ct);

            await uow.CommitAsync();   // ⬅ 一定要有這行

            return NoContent();

        }

    }
}
