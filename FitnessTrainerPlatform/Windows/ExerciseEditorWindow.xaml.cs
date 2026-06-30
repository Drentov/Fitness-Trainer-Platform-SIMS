using System.Windows;
using Microsoft.Win32;

namespace FitnessTrainerPlatform.Windows;

public partial class ExerciseEditorWindow : Window
{
    public Models.Exercise? Result { get; private set; }

    public ExerciseEditorWindow()
    {
        InitializeComponent();
    }

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Video files|*.mp4;*.avi;*.wmv;*.mov|All files|*.*"
        };
        if (dialog.ShowDialog() == true)
            VideoBox.Text = dialog.FileName;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text))
        {
            Helpers.UiHelpers.ShowError("Name is required.");
            return;
        }

        Result = new Models.Exercise
        {
            Name = NameBox.Text.Trim(),
            Description = DescBox.Text.Trim(),
            DurationDescription = DurationBox.Text.Trim(),
            VideoPath = string.IsNullOrWhiteSpace(VideoBox.Text) ? "placeholder://no-video" : VideoBox.Text.Trim()
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
