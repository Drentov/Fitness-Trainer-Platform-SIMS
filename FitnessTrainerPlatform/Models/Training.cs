namespace FitnessTrainerPlatform.Models;

public class Training
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TutelageId { get; set; } = string.Empty;
    public string TrainerId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public List<string> ExerciseIds { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public TrainingReply? Reply { get; set; }
}

public class TrainingReply
{
    public int Stars { get; set; }
    public string Opinion { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}
