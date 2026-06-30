namespace FitnessTrainerPlatform.Models;

public class TrainerProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserAccountId { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public List<string> Qualifications { get; set; } = new();
    public List<string> Prerequisites { get; set; } = new();
    public decimal TutelageFee { get; set; }
    public TrainerApprovalStatus ApprovalStatus { get; set; } = TrainerApprovalStatus.Pending;
    public bool HasPaidMonthlyFee { get; set; }
    public DateTime? LastFeePaidAt { get; set; }
    public int WarningCount { get; set; }
    public List<Review> Reviews { get; set; } = new();

    public double AverageRating =>
        Reviews.Count == 0 ? 0 : Reviews.Average(r => r.Stars);
}

public class Review
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string AuthorUserId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public int Stars { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
