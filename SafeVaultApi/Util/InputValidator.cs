// Util/InputValidator.cs

using System.Text.RegularExpressions;

namespace SafeVaultApi.Util;

public static class InputValidator
{
    public static string Sanitize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove HTML tags
        input = Regex.Replace(input, "<.*?>", string.Empty);

        return input.Trim();
    }

    public static bool IsValidUsername(string username)
    {
        return Regex.IsMatch(username, @"^[a-zA-Z0-9_-]{3,50}$");
    }

    public static bool IsValidEmail(string email)
    {
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }
}