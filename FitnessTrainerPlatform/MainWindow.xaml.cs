using System.Windows;
using FitnessTrainerPlatform.Helpers;
using FitnessTrainerPlatform.Models;
using FitnessTrainerPlatform.Views;
using FitnessTrainerPlatform.Windows;

namespace FitnessTrainerPlatform;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        LoadDashboard();
    }

    private void LoadDashboard()
    {
        var user = AppSession.CurrentUser!;
        UserLabel.Text = user.FullName;

        switch (user.Role)
        {
            case UserRole.Administrator:
                RoleLabel.Text = "Administrator Panel";
                MainContent.Content = new AdminView();
                break;
            case UserRole.Trainer:
                RoleLabel.Text = "Trainer Dashboard";
                MainContent.Content = new TrainerView();
                break;
            default:
                RoleLabel.Text = "Browse Trainers";
                MainContent.Content = new UserView();
                break;
        }
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        AppSession.CurrentUser = null;
        AppSession.CurrentTrainerProfile = null;
        var login = new LoginWindow();
        login.Show();
        Close();
    }
}
