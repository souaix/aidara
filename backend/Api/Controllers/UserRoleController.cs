using Backend.Application.Contracts.Users;
using Backend.Application.Ports;
using Backend.Infrastructure.Persistence.Postgres;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserRoleController : ControllerBase
    {
        private readonly NpgsqlDataSource _ds;
        private readonly IUserRoleRepo _userRoleRepo;
        private readonly IRoleBasisRepo _roleBasisRepo;

        public UserRoleController(
            NpgsqlDataSource ds,
            IUserRoleRepo userRoleRepo,
            IRoleBasisRepo roleBasisRepo)
        {
            _ds = ds;
            _userRoleRepo = userRoleRepo;
            _roleBasisRepo = roleBasisRepo;
        }

        /// <summary>
        /// 取得指定使用者的所有角色
        /// </summary>
        [HttpGet("{userId:guid}")]
        public async Task<ActionResult<List<UserRoleDto>>> GetUserRoles(Guid userId, CancellationToken ct)
        {
            await using var uow = new _UnitOfWork(_ds);
            var db = await uow.BeginAsync(ct);

            try
            {
                var roles = await _userRoleRepo.GetUserRolesAsync(db, userId, ct);
                await uow.CommitAsync(ct);
                return Ok(roles);
            }
            catch
            {
                await uow.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// 新增或更新使用者角色（有則更新，無則新增）
        /// </summary>
        [HttpPost("{userId:guid}")]
        public async Task<IActionResult> AddUserRole(Guid userId, [FromBody] AddUserRoleRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.RoleId))
                return BadRequest("RoleId 不可為空");

            await using var uow = new _UnitOfWork(_ds);
            var db = await uow.BeginAsync(ct);

            try
            {
                await _userRoleRepo.AddUserRoleAsync(db, userId, req.RoleId, req.ExpireDate, ct);
                await uow.CommitAsync(ct);
                return NoContent();
            }
            catch
            {
                await uow.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// 移除使用者的特定角色
        /// </summary>
        [HttpDelete("{userId:guid}/{roleId}")]
        public async Task<IActionResult> RemoveUserRole(Guid userId, string roleId, CancellationToken ct)
        {
            await using var uow = new _UnitOfWork(_ds);
            var db = await uow.BeginAsync(ct);

            try
            {
                await _userRoleRepo.RemoveUserRoleAsync(db, userId, roleId, ct);
                await uow.CommitAsync(ct);
                return NoContent();
            }
            catch
            {
                await uow.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// 查詢所有角色主檔
        /// </summary>
        [HttpGet("roles")]
        public async Task<ActionResult<List<RoleBasisDto>>> GetAllRoles(CancellationToken ct)
        {
            await using var uow = new _UnitOfWork(_ds);
            var db = await uow.BeginAsync(ct);

            try
            {
                var roles = await _roleBasisRepo.GetAllRolesAsync(db, ct);
                await uow.CommitAsync(ct);
                return Ok(roles);
            }
            catch
            {
                await uow.RollbackAsync();
                throw;
            }
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
