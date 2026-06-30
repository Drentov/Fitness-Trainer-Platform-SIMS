using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FitnessTrainerPlatform.Helpers;
using FitnessTrainerPlatform.Models;
using FitnessTrainerPlatform.Services;

namespace FitnessTrainerPlatform.Windows;

public partial class ChatWindow : Window
{
    private readonly string _tutelageId;
    private readonly HashSet<string> _displayedMessageIds = new();

    public ChatWindow(string tutelageId)
    {
        _tutelageId = tutelageId;
        InitializeComponent();
        Loaded += ChatWindow_Loaded;
        Closed += (_, _) => AppSession.ChatIpc.MessageReceived -= OnIpcMessage;
    }

    private void ChatWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var tutelage = AppSession.DataStorage.Data.Tutelages.FirstOrDefault(t => t.Id == _tutelageId);
        if (tutelage == null)
        {
            ChatTitle.Text = "Chat";
            return;
        }

        var user = AppSession.DataStorage.FindUserById(tutelage.UserId);
        var trainer = AppSession.DataStorage.FindTrainer(tutelage.TrainerId);
        var trainerUser = trainer != null ? AppSession.DataStorage.FindUserById(trainer.UserAccountId) : null;
        ChatTitle.Text = $"{user?.FullName} ↔ {trainerUser?.FullName}";

        foreach (var msg in AppSession.DataStorage.Data.ChatMessages
                     .Where(m => m.TutelageId == _tutelageId)
                     .OrderBy(m => m.SentAt))
        {
            DisplayMessage(msg, persist: false);
        }

        AppSession.ChatIpc.MessageReceived += OnIpcMessage;
    }

    private void OnIpcMessage(ChatIpcMessage ipcMsg)
    {
        if (ipcMsg.TutelageId != _tutelageId) return;

        Dispatcher.Invoke(() =>
        {
            var stored = AppSession.DataStorage.Data.ChatMessages
                .FirstOrDefault(m => m.SenderUserId == ipcMsg.SenderUserId &&
                                     m.Content == ipcMsg.Content &&
                                     Math.Abs((m.SentAt - ipcMsg.SentAt).TotalSeconds) < 2);

            if (stored != null)
                DisplayMessage(stored, persist: false);
            else
            {
                var msg = new ChatMessage
                {
                    TutelageId = ipcMsg.TutelageId,
                    SenderUserId = ipcMsg.SenderUserId,
                    SenderName = ipcMsg.SenderName,
                    Content = ipcMsg.Content,
                    SentAt = ipcMsg.SentAt
                };
                AppSession.DataStorage.Data.ChatMessages.Add(msg);
                AppSession.DataStorage.Save();
                DisplayMessage(msg, persist: false);
            }
        });
    }

    private async void Send_Click(object sender, RoutedEventArgs e) => await SendMessageAsync();

    private async void MessageBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
        {
            e.Handled = true;
            await SendMessageAsync();
        }
    }

    private async Task SendMessageAsync()
    {
        var text = MessageBox.Text.Trim();
        if (string.IsNullOrEmpty(text)) return;

        var user = AppSession.CurrentUser!;
        var msg = new ChatMessage
        {
            TutelageId = _tutelageId,
            SenderUserId = user.Id,
            SenderName = user.FullName,
            Content = text,
            SentAt = DateTime.UtcNow
        };

        AppSession.DataStorage.Data.ChatMessages.Add(msg);
        AppSession.DataStorage.Save();
        DisplayMessage(msg, persist: false);
        MessageBox.Clear();

        await AppSession.ChatIpc.BroadcastAsync(new ChatIpcMessage
        {
            TutelageId = msg.TutelageId,
            SenderUserId = msg.SenderUserId,
            SenderName = msg.SenderName,
            Content = msg.Content,
            SentAt = msg.SentAt
        });
    }

    private void DisplayMessage(ChatMessage msg, bool persist)
    {
        if (!_displayedMessageIds.Add(msg.Id)) return;

        var isMine = msg.SenderUserId == AppSession.CurrentUser?.Id;

        var container = new StackPanel
        {
            Margin = new Thickness(0, 4, 0, 4),
            HorizontalAlignment = isMine ? HorizontalAlignment.Right : HorizontalAlignment.Left,
            MaxWidth = 280
        };

        var bubble = new Border
        {
            Background = isMine
                ? new SolidColorBrush(Color.FromRgb(0, 132, 255))
                : new SolidColorBrush(Color.FromRgb(228, 230, 235)),
            CornerRadius = new CornerRadius(18),
            Padding = new Thickness(14, 10, 14, 10)
        };

        var stack = new StackPanel();
        if (!isMine)
        {
            stack.Children.Add(new TextBlock
            {
                Text = msg.SenderName,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(0, 132, 255)),
                Margin = new Thickness(0, 0, 0, 2)
            });
        }

        stack.Children.Add(new TextBlock
        {
            Text = msg.Content,
            TextWrapping = TextWrapping.Wrap,
            Foreground = isMine ? Brushes.White : new SolidColorBrush(Color.FromRgb(5, 5, 5))
        });

        stack.Children.Add(new TextBlock
        {
            Text = msg.SentAt.ToLocalTime().ToString("HH:mm"),
            FontSize = 10,
            Foreground = isMine ? new SolidColorBrush(Color.FromArgb(180, 255, 255, 255)) : new SolidColorBrush(Color.FromRgb(101, 103, 107)),
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 4, 0, 0)
        });

        bubble.Child = stack;
        container.Children.Add(bubble);
        MessagesPanel.Children.Add(container);

        MessagesScroll.ScrollToEnd();
    }
}
