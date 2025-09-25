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
    public async Task<IActionResult> Submit([FromBody] CustomerServiceQuestionnaireDto dto, CancellationToken ct)
    {
        await _svc.SubmitAsync(dto, ct);
        return NoContent();
    }
}
