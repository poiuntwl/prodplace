using System.Security.Cryptography;
using System.Text;

namespace IdentityService.Extensions;

public static class SecurityLoggerExtensions
{
    public static void LogSecurityEvent<T>(this ILogger<T> logger,
        SecurityEventIds eventId,
        Exception ex,
        string message,
        string token,
        params object[] args)
    {
        using (logger.BeginScope(new Dictionary<string, object>
               {
                   ["SecurityEventId"] = (int)eventId,
                   ["TokenHash"] = token.ToSha256(),
               }))
        {
            logger.LogError(ex, "[SECURITY {Code}] {Message}. {Args}", (int)eventId, message, args);
        }
    }

    private static string ToSha256(this string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);

        var builder = new StringBuilder();
        foreach (var h in hash)
        {
            builder.Append(h.ToString("x2"));
        }

        return builder.ToString();
    }
}

public enum SecurityEventIds
{
    RoleValidationFailure = 5001,
    InvalidTokenAttempt = 5002,
    BruteForceAttempt = 5003,
}