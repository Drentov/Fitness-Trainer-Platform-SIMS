namespace FitnessTrainerPlatform.Models;

public enum UserRole
{
    User,
    Trainer,
    Administrator
}

public enum TutelageRequestStatus
{
    Pending,
    Accepted,
    Denied
}

public enum TutelageStatus
{
    Active,
    Completed,
    Cancelled
}

public enum TrainerApprovalStatus
{
    Pending,
    Approved,
    Rejected,
    Removed
}

public enum ExerciseLocationPreference
{
    Gym,
    Home,
    Outdoors
}
