using System.Net;
using System.Security.Cryptography;
using StonksWebApp.models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace StonksWebApp.Services;

public class LoginManagerService
{
    private static LoginManagerService? _instance;
    public static LoginManagerService Instance => _instance ??= new LoginManagerService();
    private Dictionary<string, UserSession> _activeUserSessions;

    public LoginManagerService()
    {
        _activeUserSessions = new Dictionary<string, UserSession>();
    }

    public static string HashPassword(string password)
    {
        byte[] salt = new byte[128 / 8];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));

        return $"{Convert.ToBase64String(salt)}:{hashed}";
    }

    public static bool VerifyPassword(string password, string storedHashedPassword)
    {
        string[] parts = storedHashedPassword.Split(':', 2);
        byte[] salt = Convert.FromBase64String(parts[0]);
        string hashedPassword = parts[1];

        string hashedInput = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));

        return hashedPassword == hashedInput;
    }

    private string GenerateSessionToken()
    {
        byte[] randomBytes = new byte[32];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        string sessionToken = Convert.ToBase64String(randomBytes);

        return sessionToken;
    }

    public (string, bool) HandleLogin(string username, string password)
    {

        var instance = DatabaseConnectionService.Instance;
        UserModel? storedUser = instance.GetUser(username);
        if (storedUser == null)
        {
            return ("", false);
        }
        string storedPassword = storedUser.Password;

        bool valid = VerifyPassword(password, storedPassword);
        if (!valid)
        {
            return ("", false);
        }
        string sessionToken = GenerateSessionToken();
        while (_activeUserSessions.ContainsKey(sessionToken))
        {
            sessionToken = GenerateSessionToken();
        }
        _activeUserSessions.Add(sessionToken,new UserSession(username, new IPAddress(143242036), storedUser.Role));
        return (sessionToken, valid);
    }

    public (bool, UserSession?) CheckUserSessionToken(string token)
    {
        bool sessionExists = _activeUserSessions.TryGetValue(token,out UserSession? session);
        if (sessionExists)
        {
            if (session?.ValidUntil > DateTime.UtcNow)
            {
                session.ValidUntil = DateTime.UtcNow + new TimeSpan(0, 0, 30, 0);
                return (sessionExists, session);
            }
            _activeUserSessions.Remove(token);
        }
        
        return (false, session);
    }
}

public class UserSession
{
    public string Name { get; set; }
    public DateTime ValidUntil { get; set; }
    public IPAddress IpAddress { get; set; }
    public string Role { get; set; }
    public UserSession(string name, IPAddress ip, string role)
    {
        Name = name;
        IpAddress = ip;
        ValidUntil = DateTime.Now + new TimeSpan(0, 0, 30, 0);
        Role = role;
    }
}