namespace FitnessTrainerPlatform.Models;

public class UserAccount
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public string? HealthIssues { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
