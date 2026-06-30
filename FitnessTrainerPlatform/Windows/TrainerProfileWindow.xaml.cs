using System.Windows;
using System.Windows.Controls;
using FitnessTrainerPlatform.Helpers;
using FitnessTrainerPlatform.Models;

namespace FitnessTrainerPlatform.Windows;

public partial class TrainerProfileWindow : Window
{
    private readonly TrainerProfile _trainer;
    private string? _selectedTrainingId;

    public TrainerProfileWindow(TrainerProfile trainer)
    {
        _trainer = trainer;
        InitializeComponent();
        Loaded += (_, _) => Bind();
    }

    private void Bind()
    {
        var account = AppSession.DataStorage.FindUserById(_trainer.UserAccountId);
        NameText.Text = account?.FullName ?? "Trainer";
        RatingText.Text = $"{UiHelpers.StarsDisplay(_trainer.AverageRating)} ({_trainer.Reviews.Count} reviews)";
        FeeText.Text = $"Tutelage fee: ${_trainer.TutelageFee:F2} / month (does not auto-renew)";
        BioText.Text = _trainer.Bio;

        QualificationsList.ItemsSource = _trainer.Qualifications;
        PrerequisitesList.ItemsSource = _trainer.Prerequisites;
        ReviewsList.ItemsSource = _trainer.Reviews.Select(r => $"{UiHelpers.StarsDisplay(r.Stars)} — {r.AuthorName}: {r.Comment}");

        var currentUser = AppSession.CurrentUser;
        if (currentUser?.Role != UserRole.User)
        {
            UserActions.Visibility = Visibility.Collapsed;
            TrainingSection.Visibility = Visibility.Collapsed;
            return;
        }

        var activeTutelage = AppSession.DataStorage.Data.Tutelages
            .FirstOrDefault(t => t.UserId == currentUser.Id && t.TrainerId == _trainer.Id && t.Status == TutelageStatus.Active);

        if (activeTutelage != null)
        {
            TrainingSection.Visibility = Visibility.Visible;
            TrainingsList.ItemsSource = AppSession.DataStorage.Data.Trainings
                .Where(tr => tr.TutelageId == activeTutelage.Id)
                .Select(tr => new TrainingListItem
                {
                    Id = tr.Id,
                    Display = $"{tr.Title} — {(tr.Reply != null ? "Completed" : "Pending reply")}"
                }).ToList();
        }
    }

    private void RequestTutelage_Click(object sender, RoutedEventArgs e)
    {
        var user = AppSession.CurrentUser!;
        var existing = AppSession.DataStorage.Data.TutelageRequests
            .Any(r => r.UserId == user.Id && r.TrainerId == _trainer.Id && r.Status == TutelageRequestStatus.Pending);

        if (existing)
        {
            UiHelpers.ShowInfo("You already have a pending request with this trainer.");
            return;
        }

        var active = AppSession.DataStorage.Data.Tutelages
            .Any(t => t.UserId == user.Id && t.TrainerId == _trainer.Id && t.Status == TutelageStatus.Active);

        if (active)
        {
            UiHelpers.ShowInfo("You already have an active tutelage with this trainer.");
            return;
        }

        var wishSheet = new WishSheetWindow(user.HealthIssues);
        if (wishSheet.ShowDialog() != true || wishSheet.Result == null) return;

        var request = new TutelageRequest
        {
            UserId = user.Id,
            TrainerId = _trainer.Id,
            WishSheet = wishSheet.Result
        };

        AppSession.DataStorage.Data.TutelageRequests.Add(request);
        AppSession.DataStorage.Save();
        UiHelpers.ShowInfo("Tutelage request sent! The trainer will accept or deny it.");
        Close();
    }

    private void LeaveReview_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ReviewWindow(_trainer);
        dialog.ShowDialog();
        Bind();
    }

    private void TrainingsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TrainingsList.SelectedItem is TrainingListItem item)
            _selectedTrainingId = item.Id;
    }

    private void ReplyTraining_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_selectedTrainingId))
        {
            UiHelpers.ShowError("Select a training first.");
            return;
        }

        var training = AppSession.DataStorage.Data.Trainings.FirstOrDefault(t => t.Id == _selectedTrainingId);
        if (training?.Reply != null)
        {
            UiHelpers.ShowInfo("You already replied to this training.");
            return;
        }

        var dialog = new TrainingReplyWindow();
        if (dialog.ShowDialog() != true || dialog.Result == null) return;

        training!.Reply = dialog.Result;
        AppSession.DataStorage.Save();
        Bind();
        UiHelpers.ShowInfo("Training reply submitted. Thank you!");
    }

    private class TrainingListItem
    {
        public string Id { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;
        public override string ToString() => Display;
    }
}
