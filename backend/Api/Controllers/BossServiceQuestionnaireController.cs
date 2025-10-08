using Backend.Application.Services;
using Backend.Application.ViewModels.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BossServiceQuestionnaireController : ControllerBase
    {
        private readonly BossInfoQuestionnaireService _svc;

        public BossServiceQuestionnaireController(BossInfoQuestionnaireService svc)
        {
            _svc = svc;
        }

        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] BossInfoQuestionnaireDto dto, CancellationToken ct)
        {
            await _svc.SubmitAsync(dto, ct);
            return NoContent();
        }
    }
}
