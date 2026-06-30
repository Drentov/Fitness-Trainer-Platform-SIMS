using System.Globalization;
using System.Windows;
using FitnessTrainerPlatform.Helpers;
using FitnessTrainerPlatform.Models;
using FitnessTrainerPlatform.Services;

namespace FitnessTrainerPlatform.Windows;

public partial class RegisterWindow : Window
{
    public RegisterWindow()
    {
        InitializeComponent();
        RoleCombo.SelectionChanged += (_, _) =>
        {
            var isTrainer = RoleCombo.SelectedIndex == 1;
            TrainerFields.Visibility = isTrainer ? Visibility.Visible : Visibility.Collapsed;
            UserFields.Visibility = isTrainer ? Visibility.Collapsed : Visibility.Visible;
        };
    }

    private void Register_Click(object sender, RoutedEventArgs e)
    {
        var username = UsernameBox.Text.Trim();
        var password = PasswordBox.Password;
        var fullName = FullNameBox.Text.Trim();
        var email = EmailBox.Text.Trim();
        var isTrainer = RoleCombo.SelectedIndex == 1;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email))
        {
            UiHelpers.ShowError("Please fill in all required fields.");
            return;
        }

        if (AppSession.DataStorage.FindUser(username) != null)
        {
            UiHelpers.ShowError("Username already exists.");
            return;
        }

        var user = new UserAccount
        {
            Username = username,
            PasswordHash = AuthService.HashPassword(password),
            FullName = fullName,
            Email = email,
            Role = isTrainer ? UserRole.Trainer : UserRole.User,
            HealthIssues = isTrainer ? null : HealthBox.Text.Trim()
        };

        AppSession.DataStorage.Data.Users.Add(user);

        if (isTrainer)
        {
            if (!decimal.TryParse(FeeBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var fee))
                fee = 29.99m;

            var trainer = new TrainerProfile
            {
                UserAccountId = user.Id,
                Bio = BioBox.Text.Trim(),
                Qualifications = SplitList(QualificationsBox.Text),
                Prerequisites = SplitList(PrerequisitesBox.Text),
                TutelageFee = fee,
                ApprovalStatus = TrainerApprovalStatus.Pending,
                HasPaidMonthlyFee = false
            };
            AppSession.DataStorage.Data.Trainers.Add(trainer);
            UiHelpers.ShowInfo("Trainer account created. An administrator must approve you before you appear in browse listings.");
        }
        else
        {
            UiHelpers.ShowInfo("Account created. You can now log in.");
        }

        AppSession.DataStorage.Save();
        Close();
    }

    private static List<string> SplitList(string text) =>
        text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

    private void Cancel_Click(object sender, RoutedEventArgs e) => Close();
}
