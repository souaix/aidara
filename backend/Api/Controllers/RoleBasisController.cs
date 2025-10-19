// Backend.Api/Controllers/RoleBasisController.cs
using Backend.Application.Ports;
using Backend.Application.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleBasisController : ControllerBase
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IRoleBasisRepo _roleRepo;

        public RoleBasisController(IUnitOfWorkFactory uowFactory, IRoleBasisRepo roleRepo)
        {
            _uowFactory = uowFactory;
            _roleRepo = roleRepo;
        }

        /// <summary>
        /// 取得所有角色
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllRoles(CancellationToken ct)
        {
            await using var uow = await _uowFactory.BeginAsync(withTransaction: false, ct);
            var roles = await _roleRepo.GetAllRolesAsync(uow.Connection, uow.Transaction, ct);
            return Ok(roles);
        }

        /// <summary>
        /// 依角色代碼取得角色
        /// </summary>
        [HttpGet("{roleId}")]
        public async Task<IActionResult> GetRole(string roleId, CancellationToken ct)
        {
            await using var uow = await _uowFactory.BeginAsync(withTransaction: false, ct);
            var role = await _roleRepo.GetRoleAsync(uow.Connection, uow.Transaction, roleId, ct);
            return role is null ? NotFound() : Ok(role);
        }
    }
}
