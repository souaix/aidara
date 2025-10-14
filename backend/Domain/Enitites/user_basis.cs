namespace Backend.Domain.Entities;

public class user_basis
{
    public Guid user_id { get; set; }
    public string email { get; set; } = default!;
    public string? display_name { get; set; }
	public string? boss_name { get; set; }
	public string? avatar_url { get; set; }
    public string? phone { get; set; }
    public string? gender { get; set; }   // bpchar(1)
    public DateTime? birthdate { get; set; }
    public bool is_active { get; set; }
    public DateTime? last_seen_at { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
}
