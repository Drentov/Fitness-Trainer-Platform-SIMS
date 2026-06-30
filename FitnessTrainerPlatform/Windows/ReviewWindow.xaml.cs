using System.Windows;
using FitnessTrainerPlatform.Helpers;
using FitnessTrainerPlatform.Models;

namespace FitnessTrainerPlatform.Windows;

public partial class ReviewWindow : Window
{
    private readonly TrainerProfile _trainer;

    public ReviewWindow(TrainerProfile trainer)
    {
        _trainer = trainer;
        InitializeComponent();
    }

    private void Submit_Click(object sender, RoutedEventArgs e)
    {
        var user = AppSession.CurrentUser!;
        var stars = 5 - StarsCombo.SelectedIndex;

        _trainer.Reviews.Add(new Review
        {
            AuthorUserId = user.Id,
            AuthorName = user.FullName,
            Stars = stars,
            Comment = CommentBox.Text.Trim()
        });

        var stored = AppSession.DataStorage.FindTrainer(_trainer.Id);
        
        if(stored != _trainer)
        {
            if (stored != null)
            {
                stored.Reviews.Add(_trainer.Reviews.Last());
            }
        }

        AppSession.DataStorage.Save();
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
