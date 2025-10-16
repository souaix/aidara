using Backend.Application.Ports;
using Backend.Infrastructure.Persistence.Postgres;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleBasisController : ControllerBase
    {
        private readonly NpgsqlDataSource _ds;
        private readonly IRoleBasisRepo _roleRepo;

        public RoleBasisController(NpgsqlDataSource ds, IRoleBasisRepo roleRepo)
        {
            _ds = ds;
            _roleRepo = roleRepo;
        }

        /// <summary>
        /// 取得所有角色
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllRoles(CancellationToken ct)
        {
            await using var uow = new _UnitOfWork(_ds);
            var db = await uow.BeginAsync(ct);

            try
            {
                var roles = await _roleRepo.GetAllRolesAsync(db, ct);
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
        /// 依角色代碼取得角色
        /// </summary>
        [HttpGet("{roleId}")]
        public async Task<IActionResult> GetRole(string roleId, CancellationToken ct)
        {
            await using var uow = new _UnitOfWork(_ds);
            var db = await uow.BeginAsync(ct);

            try
            {
                var role = await _roleRepo.GetRoleAsync(db, roleId, ct);
                await uow.CommitAsync(ct);

                if (role is null)
                    return NotFound();

                return Ok(role);
            }
            catch
            {
                await uow.RollbackAsync();
                throw;
            }
        }
    }
}
