// !/windows-client/KConnect/ViewModels/ChatViewModel.cs
using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KConnect.Core;
using KConnect.Models;
using KConnect.Services;

namespace KConnect.ViewModels;

public partial class ChatViewModel : BaseViewModel
{
    private readonly ChatService      _chatSvc = new();
    private readonly WebSocketService _wsSvc;
    private DispatcherTimer?          _typingTimer;

    [ObservableProperty] private ObservableCollection<Conversation> _conversations = new();
    [ObservableProperty] private ObservableCollection<Message>      _messages      = new();
    [ObservableProperty] private Conversation? _selectedConversation;
    [ObservableProperty] private string _messageInput    = "";
    [ObservableProperty] private string _searchQuery     = "";
    [ObservableProperty] private string _currentUsername = "";
    [ObservableProperty] private string _typingLabel     = "";
    [ObservableProperty] private bool   _isTypingVisible;

    public event Action? LoggedOut;
    public event Action<ObservableCollection<Message>>? MessagesLoaded;

    public ChatViewModel(WebSocketService wsSvc)
    {
        _wsSvc = wsSvc;
        _wsSvc.MessageReceived += OnMessageReceived;
        _wsSvc.TypingReceived  += OnTypingReceived;
        _wsSvc.Reconnected     += async () => await LoadConversationsAsync();
        CurrentUsername = AppSession.Instance.Username ?? "";

        // Timer clears the typing indicator after 3 s of silence
        _typingTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        _typingTimer.Tick += (_, _) =>
        {
            TypingLabel     = "";
            IsTypingVisible = false;
            _typingTimer.Stop();
        };
    }

    public async Task InitializeAsync()
    {
        await _wsSvc.ConnectAsync();
        await LoadConversationsAsync();
    }

    private async Task LoadConversationsAsync()
    {
        var list = await _chatSvc.GetConversationsAsync();
        Conversations = new ObservableCollection<Conversation>(list);
    }

    partial void OnSelectedConversationChanged(Conversation? value)
    {
        if (value is null) return;
        TypingLabel     = "";
        IsTypingVisible = false;
        _ = LoadMessagesAsync(value.Id);
    }

    // Fire typing indicator while user is typing
    partial void OnMessageInputChanged(string value)
    {
        if (SelectedConversation is null) return;
        _ = _wsSvc.SendTypingAsync(SelectedConversation.Id, !string.IsNullOrEmpty(value));
    }

    private async Task LoadMessagesAsync(Guid convId)
    {
        IsBusy = true;
        var list = await _chatSvc.GetMessagesAsync(convId);
        Messages = new ObservableCollection<Message>(list);
        MessagesLoaded?.Invoke(Messages);
        IsBusy = false;
    }

    [RelayCommand]
    private async Task SendMessageAsync()
    {
        var text = MessageInput.Trim();
        if (string.IsNullOrEmpty(text) || SelectedConversation is null) return;

        MessageInput = "";
        // Clear typing indicator for ourselves immediately
        await _wsSvc.SendTypingAsync(SelectedConversation.Id, false);
        await _wsSvc.SendMessageAsync(SelectedConversation.Id, text);
    }

    [RelayCommand]
    private async Task SearchAndStartConversationAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery)) return;
        ClearMessages();

        var results = await ApiClient.Instance.GetAsync<List<User>>($"/users/search?q={Uri.EscapeDataString(SearchQuery)}");
        if (results is null || results.Count == 0)
        {
            SetError($"No user found matching '{SearchQuery}'");
            return;
        }

        var target = results[0];
        var conv   = await _chatSvc.StartConversationAsync(target.Id);
        if (conv is null) { SetError("Could not start conversation"); return; }

        if (!Conversations.Any(c => c.Id == conv.Id))
            Conversations.Insert(0, conv);

        SelectedConversation = Conversations.First(c => c.Id == conv.Id);
        SearchQuery = "";
    }

    [RelayCommand]
    private void Logout()
    {
        _wsSvc.Dispose();
        AppSession.Instance.Clear();
        LoggedOut?.Invoke();
    }

    private void OnMessageReceived(Message msg)
    {
        if (SelectedConversation?.Id == msg.ConversationId)
        {
            Messages.Add(msg);
            MessagesLoaded?.Invoke(Messages);
        }

        // Update sidebar last-message (INotifyPropertyChanged on Conversation handles UI refresh)
        var conv = Conversations.FirstOrDefault(c => c.Id == msg.ConversationId);
        if (conv != null)
        {
            conv.LastMessage = msg;

            // Bubble conversation to top of list
            var idx = Conversations.IndexOf(conv);
            if (idx > 0)
            {
                Conversations.Move(idx, 0);
            }
        }
    }

    private void OnTypingReceived(TypingEvent ev)
    {
        // Only show for the currently active conversation
        if (SelectedConversation?.Id.ToString() != ev.ConversationId) return;

        if (ev.IsTyping)
        {
            TypingLabel     = $"{ev.SenderUsername} is typing…";
            IsTypingVisible = true;
            _typingTimer?.Stop();
            _typingTimer?.Start();
        }
        else
        {
            TypingLabel     = "";
            IsTypingVisible = false;
            _typingTimer?.Stop();
        }
    }
}