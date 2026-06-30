using System.Windows;
using FitnessTrainerPlatform.Helpers;
using FitnessTrainerPlatform.Windows;

namespace FitnessTrainerPlatform;

public partial class App : Application
{
    private async void Application_Startup(object sender, StartupEventArgs e)
    {
        await AppSession.ChatIpc.StartAsync();
        var login = new LoginWindow();
        login.Show();
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        AppSession.ChatIpc.Dispose();
    }
}
