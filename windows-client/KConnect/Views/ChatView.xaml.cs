using System.Collections.ObjectModel;
using System.Windows.Controls;
using KConnect.Models;
using KConnect.Services;
using KConnect.ViewModels;

namespace KConnect.Views;

public partial class ChatView : UserControl
{
    private readonly ChatViewModel _vm;

    public ChatView()
    {
        InitializeComponent();

        var wsSvc = new WebSocketService();
        _vm = new ChatViewModel(wsSvc);
        DataContext = _vm;

        // Auto-scroll to bottom when new messages arrive
        _vm.MessagesLoaded += ScrollToBottom;

        _vm.LoggedOut += () =>
        {
            MainWindow.Instance?.NavigateTo(new LoginView());
        };

        // Wire Enter key to send
        MessageBox.KeyDown += (_, e) =>
        {
            if (e.Key == System.Windows.Input.Key.Enter && !System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftShift))
            {
                _vm.SendMessageCommand.Execute(null);
                e.Handled = true;
            }
        };

        Loaded += async (_, _) => await _vm.InitializeAsync();
    }

    private void ScrollToBottom(ObservableCollection<Message> _)
    {
        Dispatcher.BeginInvoke(() =>
        {
            MessageScroller.ScrollToBottom();
        }, System.Windows.Threading.DispatcherPriority.Background);
    }
}