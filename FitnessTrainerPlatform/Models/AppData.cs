namespace FitnessTrainerPlatform.Models;

public class AppData
{
    public List<UserAccount> Users { get; set; } = new();
    public List<TrainerProfile> Trainers { get; set; } = new();
    public List<Exercise> Exercises { get; set; } = new();
    public List<TutelageRequest> TutelageRequests { get; set; } = new();
    public List<Tutelage> Tutelages { get; set; } = new();
    public List<Training> Trainings { get; set; } = new();
    public List<ChatMessage> ChatMessages { get; set; } = new();
}
