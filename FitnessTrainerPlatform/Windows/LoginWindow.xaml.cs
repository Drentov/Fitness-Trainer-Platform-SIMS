using System.Windows;
using System.Windows.Input;
using FitnessTrainerPlatform.Helpers;
using FitnessTrainerPlatform.Models;
using FitnessTrainerPlatform.Services;

namespace FitnessTrainerPlatform.Windows;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        UsernameBox.Focus();
    }

    private void Login_Click(object sender, RoutedEventArgs e) => TryLogin();

    private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) TryLogin();
    }

    private void TryLogin()
    {
        var username = UsernameBox.Text.Trim();
        var password = PasswordBox.Password;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            UiHelpers.ShowError("Please enter username and password.");
            return;
        }

        var user = AppSession.DataStorage.FindUser(username);
        if (user == null || !AuthService.VerifyPassword(password, user.PasswordHash))
        {
            UiHelpers.ShowError("Invalid username or password.");
            return;
        }

        AppSession.CurrentUser = user;
        AppSession.CurrentTrainerProfile = user.Role == UserRole.Trainer
            ? AppSession.DataStorage.FindTrainerByUserId(user.Id)
            : null;

        var main = new MainWindow();
        main.Show();
        Close();
    }

    private void Register_Click(object sender, RoutedEventArgs e)
    {
        var register = new RegisterWindow();
        register.ShowDialog();
    }
}
