// !/windows-client/KConnect/Models/Message.cs
namespace KConnect.Models;

public class Message
{
    // Base Msg info
    public Guid   Id             { get; set; }
    public Guid   ConversationId { get; set; }
    public Guid   SenderId       { get; set; }
    public string SenderUsername { get; set; } = "";
    public string Content        { get; set; } = "";
    public DateTime CreatedAt    { get; set; }
    
    // UI Helpers
    public bool IsOwn => SenderId == Core.AppSession.Instance.UserId;
    public string TimeLabel => CreatedAt.ToLocalTime().ToString("HH:mm");
}