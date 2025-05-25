using System.Text;

namespace IntegrationTests.Utils;

public static class TestDataGenerator
{
    private static readonly Random _random = new Random();
    private static readonly object _syncLock = new object();

    // Seed data arrays
    private static readonly string[] _firstNames =
        { "John", "Jane", "Robert", "Emily", "Michael", "Sarah", "David", "Laura" };

    private static readonly string[] _lastNames =
        { "Smith", "Johnson", "Brown", "Davis", "Wilson", "Miller", "Taylor" };

    private static readonly string[] _domains = { "example.com", "test.org", "demo.net", "mock.io" };
    private static readonly string[] _streetNames = { "Main St", "Oak Ave", "Pine Rd", "Maple Dr" };
    private static readonly string[] _cities = { "New York", "Los Angeles", "Chicago", "Houston", "Phoenix" };
    private static readonly string[] _states = { "AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "FL", "GA" };

    public static string GenerateFirstName()
    {
        lock (_syncLock)
        {
            return _firstNames[_random.Next(_firstNames.Length)];
        }
    }

    public static string GenerateLastName()
    {
        lock (_syncLock)
        {
            return _lastNames[_random.Next(_lastNames.Length)];
        }
    }

    public static string GenerateEmail(string firstName = null, string lastName = null)
    {
        var fn = firstName ?? GenerateFirstName().ToLower();
        var ln = lastName ?? GenerateLastName().ToLower();

        lock (_syncLock)
        {
            return $"{fn}.{ln}{_random.Next(1000)}@{_domains[_random.Next(_domains.Length)]}";
        }
    }

    public static string GenerateUsername(string firstName = null, string lastName = null)
    {
        var fn = firstName ?? GenerateFirstName().ToLower();
        var ln = lastName ?? GenerateLastName().ToLower();

        lock (_syncLock)
        {
            return $"{fn}.{ln}{_random.Next(100)}";
        }
    }

    public static string GeneratePassword(int minLength = 8, bool includeSpecialChars = true)
    {
        const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        const string specialChars = "!@#$%^&*()_-+=[{]};:>|.,<?/";

        var chars = new List<char>();
        var length = Math.Max(minLength, 8);

        // Ensure at least one character from each required set
        chars.Add((char)_random.Next(65, 90)); // Upper case
        chars.Add((char)_random.Next(97, 122)); // Lower case
        chars.Add((char)_random.Next(48, 57)); // Digit

        if (includeSpecialChars)
        {
            chars.Add(specialChars[_random.Next(specialChars.Length)]);
            length = Math.Max(length, 12); // Longer passwords for special chars
        }

        var remaining = length - chars.Count;
        var charSet = includeSpecialChars ? validChars + specialChars : validChars;

        lock (_syncLock)
        {
            for (int i = 0; i < remaining; i++)
            {
                chars.Add(charSet[_random.Next(charSet.Length)]);
            }
        }

        return new string(chars.OrderBy(x => _random.Next()).ToArray());
    }

    public static string GeneratePhoneNumber()
    {
        lock (_syncLock)
        {
            return $"({_random.Next(200, 999)}) {_random.Next(100, 999)}-{_random.Next(1000, 9999)}";
        }
    }

    public static DateTime GenerateBirthDate(int minAge = 18, int maxAge = 100)
    {
        lock (_syncLock)
        {
            var years = _random.Next(minAge, maxAge);
            var days = _random.Next(0, 365);
            return DateTime.Now.AddYears(-years).AddDays(-days);
        }
    }

    public static string GenerateAddress()
    {
        lock (_syncLock)
        {
            return $"{_random.Next(1, 9999)} {_streetNames[_random.Next(_streetNames.Length)]}";
        }
    }

    public static string GenerateCity()
    {
        lock (_syncLock)
        {
            return _cities[_random.Next(_cities.Length)];
        }
    }

    public static string GenerateState()
    {
        lock (_syncLock)
        {
            return _states[_random.Next(_states.Length)];
        }
    }

    public static string GenerateZipCode()
    {
        lock (_syncLock)
        {
            return _random.Next(10000, 99999).ToString();
        }
    }

    private static string GenerateString(int length = 10, string characterSet = null)
    {
        if (length <= 0)
            throw new ArgumentException("Length must be a positive integer", nameof(length));

        const string defaultChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var chars = characterSet ?? defaultChars;

        if (string.IsNullOrEmpty(chars))
            throw new ArgumentException("Character set must not be empty", nameof(characterSet));

        lock (_syncLock)
        {
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)])
                .ToArray());
        }
    }

    public static string GenerateString(int length = 10,
        bool includeLowercase = true,
        bool includeUppercase = true,
        bool includeDigits = false,
        bool includeSpecial = false)
    {
        var chars = new StringBuilder();

        if (includeLowercase) chars.Append("abcdefghijklmnopqrstuvwxyz");
        if (includeUppercase) chars.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
        if (includeDigits) chars.Append("0123456789");
        if (includeSpecial) chars.Append("!@#$%^&*()_-+=[{]};:>|.,<?/");

        if (chars.Length == 0)
            throw new ArgumentException("At least one character type must be selected");

        return GenerateString(length, chars.ToString());
    }
}