// !/windows-client/kconnect/core/AppSession.cs
namespace KConnect.Core;

public class AppSession
{
    private static AppSession? _instance;
    public static AppSession Instance => _instance ??= new AppSession();
    
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public Guid UserId { get; set;  }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public bool MfaEnabled { get; set; }

    public bool IsLoggedIn => !string.IsNullOrEmpty(AccessToken);

    public void Clear()
    {
        AccessToken = null;
        RefreshToken = null;
        Username = null;
        Email = null;
    }
}