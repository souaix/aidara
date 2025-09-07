namespace Backend.Domain.Entities;

public class User
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = default!;
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Phone { get; set; }
    public string? Gender { get; set; }   // 'M','F','O','N'…
    public DateTime? Birthdate { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
