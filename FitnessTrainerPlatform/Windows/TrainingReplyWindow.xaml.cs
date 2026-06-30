using System.Windows;
using FitnessTrainerPlatform.Models;

namespace FitnessTrainerPlatform.Windows;

public partial class TrainingReplyWindow : Window
{
    public TrainingReply? Result { get; private set; }

    public TrainingReplyWindow()
    {
        InitializeComponent();
    }

    private void Submit_Click(object sender, RoutedEventArgs e)
    {
        Result = new TrainingReply
        {
            Stars = 5 - StarsCombo.SelectedIndex,
            Opinion = OpinionBox.Text.Trim()
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
