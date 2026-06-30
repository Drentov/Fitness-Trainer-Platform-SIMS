using System.Windows;
using System.Windows.Controls;
using FitnessTrainerPlatform.Helpers;
using FitnessTrainerPlatform.Models;

namespace FitnessTrainerPlatform.Views;

public partial class AdminView : UserControl
{
    public AdminView()
    {
        InitializeComponent();
        Loaded += (_, _) => Refresh();
    }

    private void Refresh()
    {
        DataPathText.Text = $"Plain JSON data file: {AppSession.DataStorage.DataFilePath}";

        var pending = AppSession.DataStorage.Data.Trainers
            .Where(t => t.ApprovalStatus == TrainerApprovalStatus.Pending)
            .Select(t =>
            {
                var user = AppSession.DataStorage.FindUserById(t.UserAccountId);
                return new TrainerAdminRow
                {
                    TrainerId = t.Id,
                    Name = user?.FullName ?? "?",
                    Email = user?.Email ?? "?",
                    Qualifications = string.Join(", ", t.Qualifications),
                    Bio = t.Bio,
                    Profile = t
                };
            }).ToList();
        PendingGrid.ItemsSource = pending;

        AllTrainersGrid.ItemsSource = AppSession.DataStorage.Data.Trainers
            .Select(t =>
            {
                var user = AppSession.DataStorage.FindUserById(t.UserAccountId);
                return new TrainerStatusRow
                {
                    TrainerId = t.Id,
                    Name = user?.FullName ?? "?",
                    Status = t.ApprovalStatus.ToString(),
                    FeePaid = t.HasPaidMonthlyFee ? "Yes" : "No",
                    Warnings = t.WarningCount.ToString(),
                    Profile = t
                };
            }).ToList();
    }

    private TrainerProfile? SelectedPending()
    {
        if (PendingGrid.SelectedItem is TrainerAdminRow row) return row.Profile;
        return null;
    }

    private TrainerProfile? SelectedAny()
    {
        if (AllTrainersGrid.SelectedItem is TrainerStatusRow row) return row.Profile;
        return null;
    }

    private void ViewDetails_Click(object sender, RoutedEventArgs e)
    {
        var t = SelectedPending();
        if (t == null) { UiHelpers.ShowError("Select a trainer."); return; }
        var user = AppSession.DataStorage.FindUserById(t.UserAccountId);
        UiHelpers.ShowInfo(
            $"Name: {user?.FullName}\nEmail: {user?.Email}\n\nQualifications:\n{string.Join("\n", t.Qualifications)}\n\nPrerequisites:\n{string.Join("\n", t.Prerequisites)}\n\nBio: {t.Bio}");
    }

    private void Approve_Click(object sender, RoutedEventArgs e)
    {
        var t = SelectedPending();
        if (t == null) { UiHelpers.ShowError("Select a pending trainer."); return; }

        if (!UiHelpers.Confirm($"Approve {AppSession.DataStorage.FindUserById(t.UserAccountId)?.FullName}? They must still pay the monthly platform fee."))
            return;

        t.ApprovalStatus = TrainerApprovalStatus.Approved;
        AppSession.DataStorage.Save();
        Refresh();
        UiHelpers.ShowInfo("Trainer approved.");
    }

    private void Reject_Click(object sender, RoutedEventArgs e)
    {
        var t = SelectedPending();
        if (t == null) { UiHelpers.ShowError("Select a pending trainer."); return; }

        t.ApprovalStatus = TrainerApprovalStatus.Rejected;
        AppSession.DataStorage.Save();
        Refresh();
    }

    private void SendWarning_Click(object sender, RoutedEventArgs e)
    {
        var t = SelectedAny();
        if (t == null) { UiHelpers.ShowError("Select a trainer."); return; }

        t.WarningCount++;
        AppSession.DataStorage.Save();
        Refresh();
        var user = AppSession.DataStorage.FindUserById(t.UserAccountId);
        UiHelpers.ShowInfo($"Warning email sent to {user?.Email} (simulated). Warning count: {t.WarningCount}");
    }

    private void RemoveTrainer_Click(object sender, RoutedEventArgs e)
    {
        var t = SelectedAny();
        if (t == null) { UiHelpers.ShowError("Select a trainer."); return; }

        if (!UiHelpers.Confirm("Remove this trainer from the platform?")) return;

        t.ApprovalStatus = TrainerApprovalStatus.Removed;
        t.HasPaidMonthlyFee = false;
        AppSession.DataStorage.Save();
        Refresh();
        UiHelpers.ShowInfo("Trainer removed from platform.");
    }

    private void ReloadData_Click(object sender, RoutedEventArgs e)
    {
        AppSession.DataStorage.Reload();
        Refresh();
        UiHelpers.ShowInfo("Data reloaded from plain JSON storage.");
    }

    private class TrainerAdminRow
    {
        public string TrainerId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Qualifications { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public TrainerProfile Profile { get; set; } = null!;
    }

    private class TrainerStatusRow
    {
        public string TrainerId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string FeePaid { get; set; } = string.Empty;
        public string Warnings { get; set; } = string.Empty;
        public TrainerProfile Profile { get; set; } = null!;
    }
}
