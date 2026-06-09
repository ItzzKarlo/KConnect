// !/windows-client/KConnect/ViewModels/ChatViewModel.cs
using System.Collections.ObjectModel;
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

    [ObservableProperty] private ObservableCollection<Conversation> _conversations = new();
    [ObservableProperty] private ObservableCollection<Message>      _messages      = new();
    [ObservableProperty] private Conversation? _selectedConversation;
    [ObservableProperty] private string _messageInput = "";
    [ObservableProperty] private string _searchQuery  = "";
    [ObservableProperty] private string _currentUsername = "";

    public event Action? LoggedOut;
    public event Action<ObservableCollection<Message>>? MessagesLoaded;

    public ChatViewModel(WebSocketService wsSvc)
    {
        _wsSvc = wsSvc;
        _wsSvc.MessageReceived += OnMessageReceived;
        CurrentUsername = AppSession.Instance.Username ?? "";
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
        _ = LoadMessagesAsync(value.Id);
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
        await _wsSvc.SendMessageAsync(SelectedConversation.Id, text);
    }

    [RelayCommand]
    private async Task SearchAndStartConversationAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery)) return;
        ClearMessages();

        // Search for the user
        var results = await ApiClient.Instance.GetAsync<List<User>>($"/users/search?q={SearchQuery}");
        if (results is null || results.Count == 0)
        {
            SetError($"No user found matching '{SearchQuery}'");
            return;
        }

        var target = results[0];
        var conv = await _chatSvc.StartConversationAsync(target.Id);
        if (conv is null) { SetError("Could not start conversation"); return; }

        // Add to list if not already there
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
        // Only append if it's for the currently open conversation
        if (SelectedConversation?.Id == msg.ConversationId)
        {
            Messages.Add(msg);
            MessagesLoaded?.Invoke(Messages);
        }

        // Update last message in sidebar
        var conv = Conversations.FirstOrDefault(c => c.Id == msg.ConversationId);
        if (conv != null) conv.LastMessage = msg;
    }
}