using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CustomerServiceQuestionnaireController : ControllerBase
{
    private readonly CustomerServiceQuestionnaireService _svc;
    public CustomerServiceQuestionnaireController(CustomerServiceQuestionnaireService svc)
    {
        _svc = svc;
    }

    [HttpPost]
    public async Task<IActionResult> Submit(Guid userId, [FromBody] CustomerServiceQuestionnaireDto dto, CancellationToken ct)
    {
        await _svc.SubmitAsync(userId, dto, ct);
        return NoContent();
    }
}
