using System.Windows;
using System.Windows.Media;

namespace FitnessTrainerPlatform.Helpers;

public static class UiHelpers
{
    public static readonly SolidColorBrush PrimaryBrush = new(Color.FromRgb(0, 132, 255));
    public static readonly SolidColorBrush BackgroundBrush = new(Color.FromRgb(240, 242, 245));
    public static readonly SolidColorBrush ChatBubbleSent = new(Color.FromRgb(0, 132, 255));
    public static readonly SolidColorBrush ChatBubbleReceived = new(Color.FromRgb(228, 230, 235));

    public static string StarsDisplay(double rating)
    {
        var full = (int)Math.Round(rating);
        full = Math.Clamp(full, 0, 5);
        return new string('★', full) + new string('☆', 5 - full);
    }

    public static void ShowError(string message) =>
        MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

    public static void ShowInfo(string message) =>
        MessageBox.Show(message, "Info", MessageBoxButton.OK, MessageBoxImage.Information);

    public static bool Confirm(string message) =>
        MessageBox.Show(message, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
}

public static class AppSession
{
    public static Services.DataStore DataStorage { get; } = new();
    public static Services.ChatIpcService ChatIpc { get; } = new();
    public static Models.UserAccount? CurrentUser { get; set; }
    public static Models.TrainerProfile? CurrentTrainerProfile { get; set; }
}
