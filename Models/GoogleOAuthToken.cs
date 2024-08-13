namespace Feast.Models;

public class GoogleOAuthToken
{
    public int Id { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime AccessTokenExpiry { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign key to ApplicationUser
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
}