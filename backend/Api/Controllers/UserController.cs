// Backend.Api/Controllers/UserController.cs
using System.Data;
using Backend.Application.Ports;
using Backend.Domain.Entities;
using Backend.Application.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IUserRepo _userRepo;

        public UserController(IUnitOfWorkFactory uowFactory, IUserRepo userRepo)
        {
            _uowFactory = uowFactory;
            _userRepo = userRepo;
        }

        /// <summary>
        /// 依 Email 查詢使用者
        /// </summary>
        [HttpGet("by-email")]
        public async Task<IActionResult> GetByEmail([FromQuery] string email, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Email 不可為空");

            await using var uow = await _uowFactory.BeginAsync(withTransaction: false, ct);
            var user = await _userRepo.GetByEmailAsync(uow.Connection, uow.Transaction, email, ct);
            return user is null ? NotFound() : Ok(user);
        }

        /// <summary>
        /// 取得使用者詳細資料（依 UserId）
        /// </summary>
        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> GetUserProfile(Guid userId, CancellationToken ct)
        {
            await using var uow = await _uowFactory.BeginAsync(withTransaction: false, ct);
            var user = await _userRepo.GetUserProfileAsync(uow.Connection, uow.Transaction, userId, ct);
            return user is null ? NotFound() : Ok(user);
        }

        /// <summary>
        /// 新增使用者
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User user, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
                return BadRequest("Email 不可為空");

            await using var uow = await _uowFactory.BeginAsync(withTransaction: true, ct);
            try
            {
                var created = await _userRepo.InsertAsync(uow.Connection, uow.Transaction, user, ct);
                await uow.CommitAsync(ct);
                return Ok(created);
            }
            catch
            {
                await uow.RollbackAsync(ct);
                throw;
            }
        }

        /// <summary>
        /// 更新使用者最後登入時間（LastSeenAt）
        /// </summary>
        [HttpPost("{userId:guid}/touch")]
        public async Task<IActionResult> TouchLastSeen(Guid userId, CancellationToken ct)
        {
            await using var uow = await _uowFactory.BeginAsync(withTransaction: true, ct);
            try
            {
                await _userRepo.TouchLastSeenAsync(uow.Connection, uow.Transaction, userId, ct);
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
}
