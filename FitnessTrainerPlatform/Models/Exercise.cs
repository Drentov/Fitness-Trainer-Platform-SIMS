namespace FitnessTrainerPlatform.Models;

public class Exercise
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TrainerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string VideoPath { get; set; } = string.Empty;
    public string DurationDescription { get; set; } = string.Empty;
}
