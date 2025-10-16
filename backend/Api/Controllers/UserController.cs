using Backend.Application.Ports;
using Backend.Domain.Entities;
using Backend.Infrastructure.Persistence.Postgres;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly NpgsqlDataSource _ds;
        private readonly IUserRepo _userRepo;

        public UserController(NpgsqlDataSource ds, IUserRepo userRepo)
        {
            _ds = ds;
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

            await using var uow = new _UnitOfWork(_ds);
            var db = await uow.BeginAsync(ct);

            try
            {
                var user = await _userRepo.GetByEmailAsync(db, email, ct);
                await uow.CommitAsync(ct);

                if (user is null)
                    return NotFound();

                return Ok(user);
            }
            catch
            {
                await uow.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// 取得使用者詳細資料（依 UserId）
        /// </summary>
        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> GetUserProfile(Guid userId, CancellationToken ct)
        {
            await using var uow = new _UnitOfWork(_ds);
            var db = await uow.BeginAsync(ct);

            try
            {
                var user = await _userRepo.GetUserProfileAsync(db, userId, ct);
                await uow.CommitAsync(ct);

                if (user is null)
                    return NotFound();

                return Ok(user);
            }
            catch
            {
                await uow.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// 新增使用者
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User user, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
                return BadRequest("Email 不可為空");

            await using var uow = new _UnitOfWork(_ds);
            var db = await uow.BeginAsync(ct);

            try
            {
                var created = await _userRepo.InsertAsync(db, user, ct);
                await uow.CommitAsync(ct);
                return Ok(created);
            }
            catch
            {
                await uow.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// 更新使用者最後登入時間（LastSeenAt）
        /// </summary>
        [HttpPost("{userId:guid}/touch")]
        public async Task<IActionResult> TouchLastSeen(Guid userId, CancellationToken ct)
        {
            await using var uow = new _UnitOfWork(_ds);
            var db = await uow.BeginAsync(ct);

            try
            {
                await _userRepo.TouchLastSeenAsync(db, userId, ct);
                await uow.CommitAsync(ct);
                return NoContent();
            }
            catch
            {
                await uow.RollbackAsync();
                throw;
            }
        }
    }
}
