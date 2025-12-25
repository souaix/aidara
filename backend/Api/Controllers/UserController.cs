// Backend.Api/Controllers/UserController.cs
using Backend.Application.Ports;
using Backend.Application.Shared;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IUserRepo _userRepo;
        private readonly IWebHostEnvironment _env;

        public UserController(IUnitOfWorkFactory uowFactory, IUserRepo userRepo, IWebHostEnvironment env)
        {
            _uowFactory = uowFactory;
            _userRepo = userRepo;
            _env = env;
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
        /// 更新使用者
        /// </summary>
        [HttpPost("{userId:guid}/profile")]
        public async Task<IActionResult> UpdateUser([FromBody] User user, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
                return BadRequest("Email 不可為空");

            await using var uow = await _uowFactory.BeginAsync(withTransaction: true, ct);
            try
            {
                var update = await _userRepo.UpdateAsync(uow.Connection, uow.Transaction, user, ct);
                await uow.CommitAsync(ct);
                return Ok(update);
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

        [HttpPost("{userId:guid}/upload-avatar")]
        public async Task<IActionResult> UploadAvatar(Guid userId, IFormFile avatar,CancellationToken ct)
        {
            if (avatar == null || avatar.Length == 0)
                return BadRequest("No file");

            // ===== 檢查副檔名 =====
            var ext = Path.GetExtension(avatar.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowed.Contains(ext))
                return BadRequest("Invalid image type");

            // ===== 儲存路徑 =====
            var backendRoot = _env.ContentRootPath;
            // backendRoot = .../App/backend/Backend.Api

            var frontendWebRoot = Path.GetFullPath(Path.Combine(
                backendRoot,
                "..", "..",            // 回到 App
                "frontend",
                "Frontend.Web",
                "wwwroot",
                "images",
                "Boss",
                "Profile"
            ));

            Directory.CreateDirectory(frontendWebRoot);

            var fileName = $"{userId}{ext}";
            var physicalPath = Path.Combine(frontendWebRoot, fileName);
            var relativePath = $"/images/Boss/Profile/{fileName}";

            // 確保資料夾存在
            Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)!);

            // ===== 寫入檔案 =====
            await using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await avatar.CopyToAsync(stream, ct);
            }

            // ===== 更新 DB（只更新 avatar_url + updated_at）=====
            await using var uow = await _uowFactory.BeginAsync(withTransaction: true, ct);
            
            try
            {
                await _userRepo.UpdateAvatarAsync(uow.Connection, uow.Transaction, userId, relativePath, ct);
                await uow.CommitAsync(ct);
                return Ok(new
                {
                    avatarUrl = relativePath
                });
            }
            catch
            {
                await uow.RollbackAsync(ct);
                throw;
            }  
        }
    }
}
