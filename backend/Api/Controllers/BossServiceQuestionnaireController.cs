using Backend.Application.Services;
using Backend.Application.ViewModels.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BossServiceQuestionnaireController : ControllerBase
    {
        private readonly BossServiceQuestionnaireService _svc;

        public BossServiceQuestionnaireController(BossServiceQuestionnaireService svc)
        {
            _svc = svc;
        }

        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] BossServiceQuestionnaireDto dto, CancellationToken ct)
        {
            await _svc.SubmitAsync(dto, ct);
            return NoContent();
        }
    }
}
