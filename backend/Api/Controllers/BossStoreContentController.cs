using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BossStoreContentController : ControllerBase
    {
        private readonly NpgsqlDataSource _ds;
        public BossStoreContentController(NpgsqlDataSource ds) => _ds = ds;

        /// <summary>
        /// 儲存/更新商店自定義內容
        /// </summary>
        [HttpPost("{userId:guid}")]
        public async Task<IActionResult> Update(Guid userId, [FromBody] StoreContentDto dto, CancellationToken ct)
        {
            const string sql = @"
        INSERT INTO store_content (user_id, content_html, created_at, updated_at)
        VALUES (@UserId, @ContentHtml, now(), now())
        ON CONFLICT (user_id)
        DO UPDATE SET content_html = EXCLUDED.content_html, updated_at = now();
    ";

            using var conn = await _ds.OpenConnectionAsync(ct);
            await conn.ExecuteAsync(sql, new { UserId = userId, dto.ContentHtml });

            return Ok();
        }


        /// <summary>
        /// 讀取商店內容
        /// </summary>
        [HttpGet("{userId:guid}")]
        public async Task<ActionResult<StoreContentDto>> Get(Guid userId, CancellationToken ct)
        {
            const string sql = @"
    SELECT user_id AS UserId, content_html AS ContentHtml
    FROM store_content
    WHERE user_id = @UserId
    LIMIT 1;
";


            using var conn = await _ds.OpenConnectionAsync(ct);
            var result = await conn.QueryFirstOrDefaultAsync<StoreContentDto>(sql, new { UserId = userId });

            if (result is null) return NotFound();
            return Ok(result);
        }

    }

    public record StoreContentDto(Guid UserId, string ContentHtml);
}
