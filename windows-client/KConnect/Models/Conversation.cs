// !/windows-client/KConnect/Models/Conversation.cs
namespace KConnect.Models;

public class Conversation
{
    public Guid     Id              { get; set; }
    public DateTime CreatedAt       { get; set; }
    public List<string> MemberUsernames { get; set; } = new();
    public Message? LastMessage     { get; set; }
    
    // Display name of other person's username
    public string DisplayName => MemberUsernames.FirstOrDefault() ?? "Unkown";
}