using Backend.Application.Ports;
using Backend.Application.ViewModels.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BossServiceMethodController : ControllerBase
    {
        private readonly IUserServiceMethodRepo _repo;

        public BossServiceMethodController(IUserServiceMethodRepo repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// 取得使用者的服務方式
        /// </summary>
        [HttpGet("{userId:guid}")]
        public async Task<ActionResult<List<UserServiceMethodVm>>> GetUserMethods(Guid userId, CancellationToken ct)
        {
            var items = await _repo.GetUserMethodsAsync(userId, ct);
            return Ok(items);
        }

        /// <summary>
        /// 更新使用者的服務方式 (覆蓋舊的)
        /// </summary>
        [HttpPost("{userId:guid}")]
        public async Task<IActionResult> UpdateUserMethods(Guid userId, [FromBody] List<string> methods, CancellationToken ct)
        {
            await _repo.UpdateUserMethodsAsync(userId, methods, ct);
            return NoContent();
        }
    }
}
