namespace Backend.Application.ViewModels.Services;

public class BossServiceMethodVm
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Method { get; set; } = "";
    public DateTime UpdatedAt { get; set; }
}
