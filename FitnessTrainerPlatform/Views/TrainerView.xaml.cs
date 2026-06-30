using System.Windows;
using System.Windows.Controls;
using FitnessTrainerPlatform.Helpers;
using FitnessTrainerPlatform.Models;
using FitnessTrainerPlatform.Windows;
using Microsoft.Win32;

namespace FitnessTrainerPlatform.Views;

public partial class TrainerView : UserControl
{
    private TrainerProfile? _trainer;
    private string? _selectedTutelageId;

    public TrainerView()
    {
        InitializeComponent();
        Loaded += (_, _) => Refresh();
    }

    private void Refresh()
    {
        _trainer = AppSession.CurrentTrainerProfile ?? AppSession.DataStorage.FindTrainerByUserId(AppSession.CurrentUser!.Id);
        if (_trainer == null) return;

        var requests = AppSession.DataStorage.Data.TutelageRequests
            .Where(r => r.TrainerId == _trainer.Id)
            .Select(r =>
            {
                var user = AppSession.DataStorage.FindUserById(r.UserId);
                return new RequestRow
                {
                    Id = r.Id,
                    UserName = user?.FullName ?? "Unknown",
                    Goals = r.WishSheet.Goals,
                    Health = r.WishSheet.HealthIssues,
                    Status = r.Status.ToString(),
                    Request = r
                };
            }).ToList();
        RequestsGrid.ItemsSource = requests;

        ExercisesList.ItemsSource = AppSession.DataStorage.Data.Exercises
            .Where(e => e.TrainerId == _trainer.Id)
            .Select(e => new ExerciseRow { Exercise = e, Display = $"{e.Name} — {e.DurationDescription}" })
            .ToList();

        ExercisePicker.ItemsSource = AppSession.DataStorage.Data.Exercises.Where(e => e.TrainerId == _trainer.Id).ToList();

        var tutelages = AppSession.DataStorage.Data.Tutelages
            .Where(t => t.TrainerId == _trainer.Id && t.Status == TutelageStatus.Active)
            .Select(t =>
            {
                var user = AppSession.DataStorage.FindUserById(t.UserId);
                return new TutelageRow { Id = t.Id, Display = $"{user?.FullName ?? "User"} (until {t.EndDate:MMM dd})" };
            }).ToList();

        TutelageCombo.ItemsSource = tutelages;
        ActiveTutelagesList.ItemsSource = tutelages;

        ApprovalStatusText.Text = $"Approval: {_trainer.ApprovalStatus}";
        FeeStatusText.Text = _trainer.HasPaidMonthlyFee
            ? $"Platform fee paid. Last payment: {_trainer.LastFeePaidAt:yyyy-MM-dd}"
            : "Platform fee NOT paid. You won't appear in browse listings until approved AND fee is paid.";
    }

    private TutelageRequest? SelectedRequest()
    {
        if (RequestsGrid.SelectedItem is RequestRow row) return row.Request;
        return null;
    }

    private void ViewWishSheet_Click(object sender, RoutedEventArgs e)
    {
        var req = SelectedRequest();
        if (req == null) { UiHelpers.ShowError("Select a request."); return; }

        var prefs = string.Join(", ", req.WishSheet.LocationPreferences);
        UiHelpers.ShowInfo(
            $"Goals: {req.WishSheet.Goals}\n\nPreferences: {prefs}\nSchedule: {req.WishSheet.ScheduleNotes}\n\nHealth: {req.WishSheet.HealthIssues}");
    }

    private void AcceptRequest_Click(object sender, RoutedEventArgs e)
    {
        var req = SelectedRequest();
        if (req == null || req.Status != TutelageRequestStatus.Pending) { UiHelpers.ShowError("Select a pending request."); return; }

        req.Status = TutelageRequestStatus.Accepted;
        var tutelage = new Tutelage
        {
            UserId = req.UserId,
            TrainerId = req.TrainerId,
            RequestId = req.Id,
            WishSheet = req.WishSheet,
            IsPaid = true
        };
        AppSession.DataStorage.Data.Tutelages.Add(tutelage);
        AppSession.DataStorage.Save();
        Refresh();
        UiHelpers.ShowInfo("Tutelage accepted! You can now chat and assign trainings.");
    }

    private void DenyRequest_Click(object sender, RoutedEventArgs e)
    {
        var req = SelectedRequest();
        if (req == null || req.Status != TutelageRequestStatus.Pending) { UiHelpers.ShowError("Select a pending request."); return; }

        req.Status = TutelageRequestStatus.Denied;
        AppSession.DataStorage.Save();
        Refresh();
    }

    private void AddExercise_Click(object sender, RoutedEventArgs e)
    {
        if (_trainer == null) return;

        var dialog = new ExerciseEditorWindow();
        if (dialog.ShowDialog() != true || dialog.Result == null) return;

        dialog.Result.TrainerId = _trainer.Id;
        AppSession.DataStorage.Data.Exercises.Add(dialog.Result);
        AppSession.DataStorage.Save();
        Refresh();
    }

    private void ShowExercise_Click(object sender, RoutedEventArgs e)
    {
        if (ExercisesList.SelectedItem is ExerciseRow row)
        {
            UiHelpers.ShowInfo(
                $"{row.Exercise.Name}\n\n{row.Exercise.Description}\n\nDuration: {row.Exercise.DurationDescription}\n\nVideo: {row.Exercise.VideoPath}\n\n(In production, this would play the trainer's recorded video.)");
        }
        else UiHelpers.ShowError("Select an exercise.");
    }

    private void CreateTraining_Click(object sender, RoutedEventArgs e)
    {
        if (TutelageCombo.SelectedItem is not TutelageRow tutelageRow)
        {
            UiHelpers.ShowError("Select an active tutelage.");
            return;
        }

        var selectedExercises = ExercisePicker.SelectedItems.Cast<Exercise>().ToList();
        if (selectedExercises.Count == 0)
        {
            UiHelpers.ShowError("Select at least one exercise.");
            return;
        }

        var tutelage = AppSession.DataStorage.Data.Tutelages.First(t => t.Id == tutelageRow.Id);
        var training = new Training
        {
            TutelageId = tutelage.Id,
            TrainerId = tutelage.TrainerId,
            UserId = tutelage.UserId,
            Title = string.IsNullOrWhiteSpace(TrainingTitleBox.Text) ? "Training Session" : TrainingTitleBox.Text.Trim(),
            ExerciseIds = selectedExercises.Select(ex => ex.Id).ToList()
        };

        AppSession.DataStorage.Data.Trainings.Add(training);
        AppSession.DataStorage.Save();
        TrainingTitleBox.Clear();
        UiHelpers.ShowInfo($"Training '{training.Title}' created with {selectedExercises.Count} exercise(s).");
    }

    private void OpenChat_Click(object sender, RoutedEventArgs e)
    {
        if (ActiveTutelagesList.SelectedItem is TutelageRow row)
            _selectedTutelageId = row.Id;

        if (string.IsNullOrEmpty(_selectedTutelageId))
        {
            UiHelpers.ShowError("Select an active tutelage.");
            return;
        }

        new ChatWindow(_selectedTutelageId).Show();
    }

    private void PayFee_Click(object sender, RoutedEventArgs e)
    {
        if (_trainer == null) return;
        if (_trainer.ApprovalStatus != TrainerApprovalStatus.Approved)
        {
            UiHelpers.ShowError("You must be approved by admin before paying.");
            return;
        }

        _trainer.HasPaidMonthlyFee = true;
        _trainer.LastFeePaidAt = DateTime.UtcNow;
        _trainer.WarningCount = 0;
        AppSession.DataStorage.Save();
        Refresh();
        UiHelpers.ShowInfo("Monthly platform fee paid (dummy transaction).");
    }

    private class RequestRow
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Goals { get; set; } = string.Empty;
        public string Health { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public TutelageRequest Request { get; set; } = null!;
    }

    private class ExerciseRow
    {
        public Exercise Exercise { get; set; } = null!;
        public string Display { get; set; } = string.Empty;
    }

    private class TutelageRow
    {
        public string Id { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;
    }
}
