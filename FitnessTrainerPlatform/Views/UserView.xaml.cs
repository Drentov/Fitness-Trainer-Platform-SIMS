using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FitnessTrainerPlatform.Helpers;
using FitnessTrainerPlatform.Models;
using FitnessTrainerPlatform.Windows;

namespace FitnessTrainerPlatform.Views;

public partial class UserView : UserControl
{
    private string? _selectedTutelageId;

    public UserView()
    {
        InitializeComponent();
        Loaded += (_, _) => Refresh();
    }

    private void Refresh()
    {
        var user = AppSession.CurrentUser!;
        HealthBox.Text = user.HealthIssues ?? string.Empty;

        var trainers = AppSession.DataStorage.GetApprovedTrainers()
            .Select(t =>
            {
                var account = AppSession.DataStorage.FindUserById(t.UserAccountId);
                return new TrainerCardVm
                {
                    Trainer = t,
                    DisplayName = account?.FullName ?? "Unknown",
                    Bio = t.Bio,
                    RatingText = $"{UiHelpers.StarsDisplay(t.AverageRating)} ({t.Reviews.Count} reviews)",
                    FeeText = $"${t.TutelageFee:F2} / month tutelage"
                };
            }).ToList();

        TrainersList.ItemsSource = trainers;

        var tutelages = AppSession.DataStorage.Data.Tutelages
            .Where(tu => tu.UserId == user.Id && tu.Status == TutelageStatus.Active)
            .Select(tu =>
            {
                var trainer = AppSession.DataStorage.FindTrainer(tu.TrainerId);
                var trainerUser = trainer != null ? AppSession.DataStorage.FindUserById(trainer.UserAccountId) : null;
                return $"{trainerUser?.FullName ?? "Trainer"} (until {tu.EndDate:MMM dd})";
            }).ToList();

        MyTutelagesList.ItemsSource = AppSession.DataStorage.Data.Tutelages
            .Where(tu => tu.UserId == user.Id && tu.Status == TutelageStatus.Active)
            .Select(tu =>
            {
                var trainer = AppSession.DataStorage.FindTrainer(tu.TrainerId);
                var trainerUser = trainer != null ? AppSession.DataStorage.FindUserById(trainer.UserAccountId) : null;
                return new TutelageListItem
                {
                    Id = tu.Id,
                    Display = $"{trainerUser?.FullName ?? "Trainer"} (until {tu.EndDate:MMM dd})"
                };
            }).ToList();
    }

    private void SaveProfile_Click(object sender, RoutedEventArgs e)
    {
        AppSession.CurrentUser!.HealthIssues = HealthBox.Text.Trim();
        var user = AppSession.DataStorage.FindUserById(AppSession.CurrentUser.Id);
        if (user != null) user.HealthIssues = HealthBox.Text.Trim();
        AppSession.DataStorage.Save();
        UiHelpers.ShowInfo("Profile updated.");
    }

    private void ViewProfile_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is TrainerCardVm vm)
            OpenTrainerProfile(vm.Trainer);
    }

    private void TrainerCard_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is TrainerCardVm vm)
            OpenTrainerProfile(vm.Trainer);
    }

    private void OpenTrainerProfile(TrainerProfile trainer)
    {
        var dialog = new TrainerProfileWindow(trainer);
        dialog.ShowDialog();
        Refresh();
    }

    private void MyTutelagesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MyTutelagesList.SelectedItem is TutelageListItem item)
            _selectedTutelageId = item.Id;
    }

    private void OpenChat_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_selectedTutelageId))
        {
            UiHelpers.ShowError("Select an active tutelage first.");
            return;
        }

        var chat = new ChatWindow(_selectedTutelageId);
        chat.Show();
    }

    private class TrainerCardVm
    {
        public TrainerProfile Trainer { get; set; } = null!;
        public string DisplayName { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string RatingText { get; set; } = string.Empty;
        public string FeeText { get; set; } = string.Empty;
    }

    private class TutelageListItem
    {
        public string Id { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;

        public override string ToString() => Display;
    }
}
