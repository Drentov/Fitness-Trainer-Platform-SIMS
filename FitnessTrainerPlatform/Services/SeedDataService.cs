using FitnessTrainerPlatform.Models;

namespace FitnessTrainerPlatform.Services;

public static class SeedDataService
{
    public static AppData CreateInitialData()
    {
        var admin = new UserAccount
        {
            Id = "admin-001",
            Username = "admin",
            PasswordHash = AuthService.HashPassword("admin123"),
            FullName = "System Administrator",
            Email = "admin@fittrain.local",
            Role = UserRole.Administrator
        };

        var user1 = new UserAccount
        {
            Id = "user-001",
            Username = "john",
            PasswordHash = AuthService.HashPassword("user123"),
            FullName = "John Smith",
            Email = "john@example.com",
            Role = UserRole.User,
            HealthIssues = "Mild knee pain when running"
        };

        var user2 = new UserAccount
        {
            Id = "user-002",
            Username = "maria",
            PasswordHash = AuthService.HashPassword("user123"),
            FullName = "Maria Garcia",
            Email = "maria@example.com",
            Role = UserRole.User
        };

        var trainerUser1 = new UserAccount
        {
            Id = "trainer-user-001",
            Username = "coach_mike",
            PasswordHash = AuthService.HashPassword("trainer123"),
            FullName = "Mike Johnson",
            Email = "mike@fittrain.local",
            Role = UserRole.Trainer
        };

        var trainerUser2 = new UserAccount
        {
            Id = "trainer-user-002",
            Username = "coach_anna",
            PasswordHash = AuthService.HashPassword("trainer123"),
            FullName = "Anna Kovacs",
            Email = "anna@fittrain.local",
            Role = UserRole.Trainer
        };

        var pendingTrainerUser = new UserAccount
        {
            Id = "trainer-user-003",
            Username = "coach_new",
            PasswordHash = AuthService.HashPassword("trainer123"),
            FullName = "Peter Novak",
            Email = "peter@fittrain.local",
            Role = UserRole.Trainer
        };

        var trainer1 = new TrainerProfile
        {
            Id = "trainer-001",
            UserAccountId = trainerUser1.Id,
            Bio = "Strength & conditioning specialist with 10 years of experience.",
            Qualifications = ["Certified Personal Trainer", "Physical Education degree", "Swimming instructor license"],
            Prerequisites = ["Must NOT have uncontrolled high blood pressure", "Must know how to swim for pool sessions"],
            TutelageFee = 49.99m,
            ApprovalStatus = TrainerApprovalStatus.Approved,
            HasPaidMonthlyFee = true,
            LastFeePaidAt = DateTime.UtcNow.AddDays(-5),
            Reviews =
            [
                new Review
                {
                    AuthorUserId = user1.Id,
                    AuthorName = user1.FullName,
                    Stars = 5,
                    Comment = "Excellent trainer! Very motivating and knowledgeable."
                },
                new Review
                {
                    AuthorUserId = user2.Id,
                    AuthorName = user2.FullName,
                    Stars = 4,
                    Comment = "Great workouts, sometimes a bit intense."
                }
            ]
        };

        var trainer2 = new TrainerProfile
        {
            Id = "trainer-002",
            UserAccountId = trainerUser2.Id,
            Bio = "Tai chi and flexibility expert. Focus on mindful movement.",
            Qualifications = ["Tai Chi diploma", "Yoga instructor certification"],
            Prerequisites = ["No severe joint injuries"],
            TutelageFee = 39.99m,
            ApprovalStatus = TrainerApprovalStatus.Approved,
            HasPaidMonthlyFee = true,
            LastFeePaidAt = DateTime.UtcNow.AddDays(-2),
            Reviews =
            [
                new Review
                {
                    AuthorUserId = user2.Id,
                    AuthorName = user2.FullName,
                    Stars = 5,
                    Comment = "Calming and effective sessions."
                }
            ]
        };

        var pendingTrainer = new TrainerProfile
        {
            Id = "trainer-003",
            UserAccountId = pendingTrainerUser.Id,
            Bio = "New to the platform. CrossFit enthusiast.",
            Qualifications = ["CrossFit Level 1"],
            Prerequisites = [],
            TutelageFee = 29.99m,
            ApprovalStatus = TrainerApprovalStatus.Pending,
            HasPaidMonthlyFee = false
        };

        var exercises = new List<Exercise>
        {
            new()
            {
                Id = "ex-001",
                TrainerId = trainer1.Id,
                Name = "Push-ups",
                Description = "Standard push-ups with proper form. Keep core tight.",
                VideoPath = "placeholder://pushups",
                DurationDescription = "3 sets of 20 reps"
            },
            new()
            {
                Id = "ex-002",
                TrainerId = trainer1.Id,
                Name = "Freestyle Swimming",
                Description = "Swim freestyle focusing on breathing rhythm.",
                VideoPath = "placeholder://swimming",
                DurationDescription = "15 minutes continuous"
            },
            new()
            {
                Id = "ex-003",
                TrainerId = trainer2.Id,
                Name = "Tai Chi Warm-up",
                Description = "Gentle flowing movements to warm up joints.",
                VideoPath = "placeholder://taichi",
                DurationDescription = "10 minutes"
            }
        };

        return new AppData
        {
            Users = [admin, user1, user2, trainerUser1, trainerUser2, pendingTrainerUser],
            Trainers = [trainer1, trainer2, pendingTrainer],
            Exercises = exercises
        };
    }
}
