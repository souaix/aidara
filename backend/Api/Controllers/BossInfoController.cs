using Backend.Application.Contracts.Users;
using Backend.Application.Ports;
using Backend.Application.Services.Boss;
using Backend.Application.ViewModels;
using Backend.Application.ViewModels.Services;
using Backend.Contracts.Users;
using Backend.Domain.Entities;
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


        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetUserProfile(Guid userId, CancellationToken ct)
        {
            var userRepo = _uow.CreateUserRepo();

            var user = await userRepo.GetUserProfileAsync(userId, ct);
            var role = await userRepo.GetUserRoleAsync(userId, ct);

            if (user == null)
                return NotFound();

            var dto = new
            {
                user.UserId,
                user.Email,
                DisplayName = role?.RoleId?.Contains("BOSS") == true
                    ? user.BossName
                    : user.DisplayName,
                user.AvatarUrl
            };

            return Ok(dto);
        }

        [HttpGet("role/{userId:guid}")]
        public async Task<IActionResult> GetUserRole(Guid userId, CancellationToken ct)
        {
            var repo = _uow.CreateUserRepo();
            var roles = await repo.GetUserRolesAsync(userId, ct);

            if (roles is null || !roles.Any())
                return Ok(new UserRoleDto { RoleId = "UNVERIFYBOSS", RoleName = "未認證" });

            var mainRole = roles.First();
            return Ok(new UserRoleDto
            {
                RoleId = mainRole.RoleId,
                RoleName = mainRole.RoleId == "UNVERIFYBOSS" ? "未認證" : mainRole.RoleName
            });
        }

        // 🔹 3) 地址資訊（含地名轉換）
        //[HttpGet("address/{userId:guid}")]
        //public async Task<IActionResult> GetUserAddress(Guid userId, CancellationToken ct)
        //{
        //    var repo = _uow.CreateBossInfoRepo();
        //    var addr = await repo.GetBossAddressAsync(userId, ct);

        //    if (addr is null)
        //        return Ok(new BossUserAddressDto { CityName = "", DistrictName = "", Phone = "", Street = "", AddressNo = "" });

        //    return Ok(addr);
        //}


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
