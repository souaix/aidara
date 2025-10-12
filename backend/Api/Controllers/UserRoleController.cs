using Backend.Application.Ports;
using Backend.Application.ViewModels.Users;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserRoleController : ControllerBase
    {
        private readonly IUserRoleRepo _userRoleRepo;
        private readonly IRoleBasisRepo _roleBasisRepo;

        public UserRoleController(IUserRoleRepo userRoleRepo, IRoleBasisRepo roleBasisRepo)
        {
            _userRoleRepo = userRoleRepo;
            _roleBasisRepo = roleBasisRepo;
        }

        /// <summary>
        /// 取得指定使用者的所有角色
        /// </summary>
        [HttpGet("{userId:guid}")]
        public async Task<ActionResult<List<UserRoleDto>>> GetUserRoles(Guid userId, CancellationToken ct)
        {
            var roles = await _userRoleRepo.GetUserRolesAsync(userId, ct);
            return Ok(roles);
        }

        /// <summary>
        /// 新增或更新使用者角色（有則更新，無則新增）
        /// </summary>
        [HttpPost("{userId:guid}")]
        public async Task<IActionResult> AddUserRole(Guid userId, [FromBody] AddUserRoleRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.RoleId))
                return BadRequest("RoleId 不可為空");

            await _userRoleRepo.AddUserRoleAsync(userId, req.RoleId, req.ExpireDate, ct);
            return NoContent();
        }

        /// <summary>
        /// 移除使用者的特定角色
        /// </summary>
        [HttpDelete("{userId:guid}/{roleId}")]
        public async Task<IActionResult> RemoveUserRole(Guid userId, string roleId, CancellationToken ct)
        {
            await _userRoleRepo.RemoveUserRoleAsync(userId, roleId, ct);
            return NoContent();
        }

        /// <summary>
        /// 查詢所有角色主檔
        /// </summary>
        [HttpGet("roles")]
        public async Task<ActionResult<List<RoleBasisDto>>> GetAllRoles(CancellationToken ct)
        {
            var roles = await _roleBasisRepo.GetAllRolesAsync(ct);
            return Ok(roles);
        }
    }

    /// <summary>
    /// 用於新增或更新使用者角色的 Request DTO
    /// </summary>
    public class AddUserRoleRequest
    {
        /// <summary>角色代碼（如 ADMIN、BOSS、CUSTOMER）</summary>
        public string RoleId { get; set; } = string.Empty;

        /// <summary>角色到期日（可為 null 表示永久有效）</summary>
        public DateTime? ExpireDate { get; set; }
    }
}
