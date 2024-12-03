using AuthApi.Models;

public class RefreshToken
{
    public string Token { get; set; }
    public int UserId { get; set; }
    public virtual AppUser? User { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}