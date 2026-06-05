// !/windows-client/kconnect/Models/User.cs
namespace KConnect.Models;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public bool MfaEnabled { get; set; }
    public bool IsVerified { get; set; }
}