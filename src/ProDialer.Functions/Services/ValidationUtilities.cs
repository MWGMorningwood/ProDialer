using System.Text.RegularExpressions;

namespace ProDialer.Functions.Services;

/// <summary>
/// Static utility class for phone number and timezone validation
/// Provides helper methods without the overhead of a full service layer
/// </summary>
public static class ValidationUtilities
{
    /// <summary>
    /// Validates a phone number using basic NANPA format rules
    /// </summary>
    /// <param name="phoneNumber">Phone number to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // Remove all non-digit characters
        var digitsOnly = Regex.Replace(phoneNumber, @"[^\d]", "");

        // Basic NANPA validation (10 or 11 digits)
        if (digitsOnly.Length == 10)
        {
            // 10-digit format: NXXNXXXXXX
            return digitsOnly[0] >= '2' && digitsOnly[3] >= '2';
        }
        
        if (digitsOnly.Length == 11 && digitsOnly[0] == '1')
        {
            // 11-digit format with country code: 1NXXNXXXXXX
            return digitsOnly[1] >= '2' && digitsOnly[4] >= '2';
        }

        return false;
    }

    /// <summary>
    /// Extracts timezone information from a US/Canada area code
    /// </summary>
    /// <param name="phoneNumber">Phone number to analyze</param>
    /// <returns>TimeZoneInfo or null if not determinable</returns>
    public static TimeZoneInfo? GetTimezoneFromAreaCode(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return null;

        var digitsOnly = Regex.Replace(phoneNumber, @"[^\d]", "");
        if (digitsOnly.Length < 10)
            return null;

        // Extract area code (first 3 digits for 10-digit, or digits 1-3 for 11-digit)
        var areaCode = digitsOnly.Length == 11 && digitsOnly[0] == '1' 
            ? digitsOnly.Substring(1, 3) 
            : digitsOnly.Substring(0, 3);

        // Simple area code to timezone mapping (subset for demonstration)
        return areaCode switch
        {
            "212" or "718" or "917" or "646" => TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"), // NYC
            "213" or "323" or "310" or "424" => TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"), // LA
            "312" or "773" or "872" => TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"), // Chicago
            "602" or "623" or "480" => TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"), // Phoenix
            _ => null
        };
    }

    /// <summary>
    /// Checks if the current time falls within calling hours for a given timezone
    /// </summary>
    /// <param name="timezone">Timezone to check</param>
    /// <param name="startHour">Start hour (24-hour format, default 8 AM)</param>
    /// <param name="endHour">End hour (24-hour format, default 9 PM)</param>
    /// <returns>True if within calling hours</returns>
    public static bool IsWithinCallingHours(TimeZoneInfo timezone, int startHour = 8, int endHour = 21)
    {
        if (timezone == null)
            return false;

        var currentTimeInZone = TimeZoneInfo.ConvertTime(DateTime.Now, timezone);
        var currentHour = currentTimeInZone.Hour;

        return currentHour >= startHour && currentHour < endHour;
    }

    /// <summary>
    /// Normalizes a phone number to digits only
    /// </summary>
    /// <param name="phoneNumber">Phone number to normalize</param>
    /// <returns>Digits-only phone number</returns>
    public static string NormalizePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return string.Empty;

        return Regex.Replace(phoneNumber, @"[^\d]", "");
    }

    /// <summary>
    /// Checks if a phone number is likely a mobile number based on common patterns
    /// </summary>
    /// <param name="phoneNumber">Phone number to check</param>
    /// <returns>True if likely mobile, false otherwise</returns>
    public static bool IsLikelyMobileNumber(string phoneNumber)
    {
        var normalized = NormalizePhoneNumber(phoneNumber);
        if (normalized.Length < 10)
            return false;

        // This is a simplified heuristic - in production you'd use a proper carrier lookup service
        // Mobile numbers often start with certain area codes or have specific patterns
        var areaCode = normalized.Length == 11 && normalized[0] == '1' 
            ? normalized.Substring(1, 3) 
            : normalized.Substring(0, 3);

        // Example mobile-heavy area codes (this is not comprehensive)
        var mobileHeavyAreaCodes = new[] { "917", "646", "929", "332" }; // NYC mobile-heavy codes
        return mobileHeavyAreaCodes.Contains(areaCode);
    }
}
