// Backend.Api/Controllers/UserRoleController.cs
using Backend.Application.Contracts.User;
using Backend.Application.Ports;
using Backend.Application.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserRoleController : ControllerBase
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IUserRoleRepo _userRoleRepo;
        private readonly IRoleBasisRepo _roleBasisRepo;
        private readonly IUserActiveModeRepo _modeRepo;

        public UserRoleController(
            IUnitOfWorkFactory uowFactory,
            IUserRoleRepo userRoleRepo,
            IRoleBasisRepo roleBasisRepo,
            IUserActiveModeRepo modeRepo)
        {
            _uowFactory = uowFactory;
            _userRoleRepo = userRoleRepo;
            _roleBasisRepo = roleBasisRepo;
            _modeRepo = modeRepo;
        }

        /// <summary>
        /// 取得指定使用者的所有角色（僅 roleId）
        /// </summary>
        [HttpGet("user/{userId:guid}/roles")]
        public async Task<ActionResult<List<UserRoleDto>>> GetUserRoles(Guid userId, CancellationToken ct)
        {
            await using var uow = await _uowFactory.BeginAsync(withTransaction: false, ct);
            var roles = await _userRoleRepo.GetUserRolesAsync(uow.Connection, uow.Transaction, userId, ct);
            return Ok(roles ?? new List<UserRoleDto>());
        }

        /// <summary>
        /// 新增或更新使用者角色（有則更新，無則新增）
        /// </summary>
        [HttpPost("{userId:guid}/roles")]
        public async Task<IActionResult> AddUserRole(Guid userId, [FromBody] AddUserRoleRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.RoleId))
                return BadRequest("RoleId 不可為空");

            await using var uow = await _uowFactory.BeginAsync(withTransaction: true, ct);
            try
            {
                await _userRoleRepo.AddUserRoleAsync(uow.Connection, uow.Transaction, userId, req.RoleId, req.ExpireDate, ct);
                await uow.CommitAsync(ct);
                return NoContent();
            }
            catch
            {
                await uow.RollbackAsync(ct);
                throw;
            }
        }

        /// <summary>
        /// 移除使用者的特定角色
        /// </summary>
        [HttpDelete("{userId:guid}/roles/{roleId}")]
        public async Task<IActionResult> RemoveUserRole(Guid userId, string roleId, CancellationToken ct)
        {
            await using var uow = await _uowFactory.BeginAsync(withTransaction: true, ct);
            try
            {
                await _userRoleRepo.RemoveUserRoleAsync(uow.Connection, uow.Transaction, userId, roleId, ct);
                await uow.CommitAsync(ct);
                return NoContent();
            }
            catch
            {
                await uow.RollbackAsync(ct);
                throw;
            }
        }

        /// <summary>
        /// 查詢所有角色主檔
        /// </summary>
        [HttpGet("roles")]
        public async Task<ActionResult<List<RoleBasisDto>>> GetAllRoles(CancellationToken ct)
        {
            await using var uow = await _uowFactory.BeginAsync(withTransaction: false, ct);
            var roles = await _roleBasisRepo.GetAllRolesAsync(uow.Connection, uow.Transaction, ct);
            return Ok(roles ?? new List<RoleBasisDto>());
        }

        /// <summary>
        /// 取得使用者可用角色清單（含中文名稱）
        /// </summary>
        [HttpGet("user/{userId:guid}/available-roles")]
        public async Task<ActionResult<List<UserRoleWithNameDto>>> GetAvailableRoles(Guid userId, CancellationToken ct)
        {
            await using var uow = await _uowFactory.BeginAsync(withTransaction: false, ct);
            var roles = await _userRoleRepo.GetUserRolesWithNameAsync(uow.Connection, uow.Transaction, userId, ct);
            return Ok(roles ?? new List<UserRoleWithNameDto>());
        }

        /// <summary>
        /// 取得指定使用者目前的操作模式
        /// </summary>
        [HttpGet("user/{userId:guid}/active-mode")]
        public async Task<ActionResult<ActiveModeDto>> GetActiveMode(Guid userId, CancellationToken ct)
        {
            await using var uow = await _uowFactory.BeginAsync(withTransaction: false, ct);
            var mode = await _modeRepo.GetActiveModeAsync(uow.Connection, uow.Transaction, userId, ct)
                        ?? "CUSTOMER";
            return Ok(new ActiveModeDto { RoleId = mode });
        }

        /// <summary>
        /// 設定指定使用者的操作模式
        /// </summary>
        [HttpPost("user/{userId:guid}/switch-active-mode")]
        public async Task<IActionResult> SetActiveMode(Guid userId, [FromBody] ActiveModeDto req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.RoleId))
                return BadRequest("RoleId 不可為空");

            await using var uow = await _uowFactory.BeginAsync(withTransaction: true, ct);
            try
            {
                await _modeRepo.SetActiveModeAsync(uow.Connection, uow.Transaction, userId, req.RoleId, ct);
                await uow.CommitAsync(ct);
                return NoContent();
            }
            catch
            {
                await uow.RollbackAsync(ct);
                throw;
            }
        }
    }

    /// <summary>
    /// 用於新增或更新使用者角色的 Request DTO
    /// </summary>
    public class AddUserRoleRequest
    {
        public string RoleId { get; set; } = string.Empty;
        public DateTime? ExpireDate { get; set; }
    }

    /// <summary>
    /// 用於取得／設定使用者目前模式
    /// </summary>
    public class ActiveModeDto
    {
        public string RoleId { get; set; } = "CUSTOMER";
    }
}
