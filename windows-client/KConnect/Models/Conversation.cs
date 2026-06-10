// !/windows-client/KConnect/Models/Conversation.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KConnect.Models;

public class Conversation : INotifyPropertyChanged
{
    private Message? _lastMessage;

    public Guid     Id              { get; set; }
    public DateTime CreatedAt       { get; set; }
    public List<string> MemberUsernames { get; set; } = new();

    public Message? LastMessage
    {
        get => _lastMessage;
        set
        {
            if (_lastMessage == value) return;
            _lastMessage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(LastMessagePreview));
        }
    }

    // Truncated preview shown in sidebar
    public string LastMessagePreview =>
        LastMessage?.Content is { Length: > 0 } c
            ? (c.Length > 60 ? c[..57] + "…" : c)
            : "No messages yet";

    // The display name is the other person's username(s)
    public string DisplayName => MemberUsernames.FirstOrDefault() ?? "Unknown";

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}