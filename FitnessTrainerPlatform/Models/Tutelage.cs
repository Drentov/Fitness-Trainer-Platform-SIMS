namespace FitnessTrainerPlatform.Models;

public class UserWishSheet
{
    public string Goals { get; set; } = string.Empty;
    public List<ExerciseLocationPreference> LocationPreferences { get; set; } = new();
    public string HealthIssues { get; set; } = string.Empty;
    public string ScheduleNotes { get; set; } = string.Empty;
}

public class TutelageRequest
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string TrainerId { get; set; } = string.Empty;
    public UserWishSheet WishSheet { get; set; } = new();
    public TutelageRequestStatus Status { get; set; } = TutelageRequestStatus.Pending;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
}

public class Tutelage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string TrainerId { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public UserWishSheet WishSheet { get; set; } = new();
    public TutelageStatus Status { get; set; } = TutelageStatus.Active;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; } = DateTime.UtcNow.AddMonths(1);
    public bool IsPaid { get; set; }
}
