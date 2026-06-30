using System.Windows;
using FitnessTrainerPlatform.Models;

namespace FitnessTrainerPlatform.Windows;

public partial class WishSheetWindow : Window
{
    public UserWishSheet? Result { get; private set; }

    public WishSheetWindow(string? existingHealth)
    {
        InitializeComponent();
        if (!string.IsNullOrWhiteSpace(existingHealth))
            HealthBox.Text = existingHealth;
    }

    private void Submit_Click(object sender, RoutedEventArgs e)
    {
        var prefs = new List<ExerciseLocationPreference>();
        if (GymCheck.IsChecked == true) prefs.Add(ExerciseLocationPreference.Gym);
        if (HomeCheck.IsChecked == true) prefs.Add(ExerciseLocationPreference.Home);
        if (OutdoorsCheck.IsChecked == true) prefs.Add(ExerciseLocationPreference.Outdoors);

        Result = new UserWishSheet
        {
            Goals = GoalsBox.Text.Trim(),
            LocationPreferences = prefs,
            ScheduleNotes = ScheduleBox.Text.Trim(),
            HealthIssues = HealthBox.Text.Trim()
        };

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
