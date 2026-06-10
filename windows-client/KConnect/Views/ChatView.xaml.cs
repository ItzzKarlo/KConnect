using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
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

        _vm.MessagesLoaded += ScrollToBottom;

        _vm.LoggedOut += () =>
        {
            MainWindow.Instance?.NavigateTo(new LoginView());
        };

        // Enter sends, Shift+Enter is reserved (AcceptsReturn=False anyway)
        MsgInput.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter && !Keyboard.IsKeyDown(Key.LeftShift))
            {
                _vm.SendMessageCommand.Execute(null);
                e.Handled = true;
            }
        };

        // Also allow Enter in the search box to trigger search
        SearchBox.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter)
            {
                _vm.SearchAndStartConversationCommand.Execute(null);
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
