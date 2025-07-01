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

        // Comprehensive area code to timezone mapping
        return areaCode switch
        {
            // Eastern Time Zone
            "201" or "202" or "203" or "207" or "212" or "215" or "216" or "217" or "234" or "239" or
            "240" or "248" or "267" or "269" or "270" or "276" or "301" or "302" or "304" or "305" or
            "313" or "315" or "321" or "330" or "347" or "352" or "386" or "401" or "404" or "407" or
            "410" or "412" or "413" or "414" or "419" or "423" or "434" or "440" or "443" or "470" or
            "475" or "478" or "484" or "508" or "513" or "516" or "517" or "518" or "561" or "567" or
            "570" or "571" or "585" or "586" or "607" or "609" or "610" or "614" or "615" or "616" or
            "617" or "618" or "631" or "646" or "678" or "680" or "703" or "704" or "706" or "708" or
            "716" or "717" or "718" or "724" or "727" or "732" or "734" or "740" or "754" or "757" or
            "762" or "772" or "774" or "781" or "786" or "803" or "804" or "813" or "828" or "843" or
            "845" or "848" or "850" or "856" or "857" or "859" or "860" or "862" or "863" or "865" or
            "904" or "908" or "910" or "912" or "914" or "917" or "919" or "929" or "931" or "937" or
            "941" or "947" or "954" or "959" or "973" or "980" or "984" => 
                TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"),

            // Central Time Zone
            "205" or "214" or "218" or "219" or "224" or "225" or "228" or "251" or "254" or "256" or
            "281" or "309" or "312" or "316" or "318" or "319" or "320" or "334" or "337" or "361" or
            "409" or "414" or "417" or "430" or "432" or "469" or "479" or "501" or "504" or "507" or
            "512" or "515" or "563" or "573" or "580" or "601" or "605" or "608" or "612" or "620" or
            "630" or "636" or "641" or "651" or "660" or "662" or "682" or "701" or "712" or "713" or
            "715" or "731" or "763" or "773" or "779" or "785" or "806" or "815" or "816" or "817" or
            "832" or "847" or "855" or "870" or "872" or "903" or "918" or "920" or "936" or "940" or
            "952" or "956" or "972" or "979" or "985" => 
                TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"),

            // Mountain Time Zone
            "303" or "307" or "385" or "406" or "435" or "480" or "505" or "520" or "575" or "602" or
            "623" or "720" or "801" or "802" or "830" or "915" or "928" or "970" => 
                TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"),

            // Pacific Time Zone  
            "206" or "209" or "213" or "253" or "279" or "310" or "323" or "341" or "350" or "360" or
            "408" or "415" or "424" or "442" or "510" or "530" or "559" or "562" or "619" or "626" or
            "628" or "650" or "657" or "661" or "669" or "707" or "714" or "747" or "760" or "805" or
            "818" or "831" or "858" or "909" or "916" or "925" or "949" or "951" => 
                TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"),

            // Alaska Time Zone
            "907" => TimeZoneInfo.FindSystemTimeZoneById("Alaskan Standard Time"),

            // Hawaii Time Zone
            "808" => TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time"),

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

        // Extract area code
        var areaCode = normalized.Length == 11 && normalized[0] == '1' 
            ? normalized.Substring(1, 3) 
            : normalized.Substring(0, 3);

        // Mobile-heavy area codes (wireless-first allocations)
        var mobileHeavyAreaCodes = new[]
        {
            // New York metro mobile-heavy codes
            "917", "646", "929", "332",
            // California mobile-heavy codes  
            "424", "628", "747", "279",
            // Texas mobile-heavy codes
            "469", "682", "430", "945",
            // Florida mobile-heavy codes
            "754", "786", "689",
            // Other major mobile-heavy codes
            "470", "678", "404", // Atlanta
            "240", "301", // Maryland
            "571", "703", // Virginia
        };

        return mobileHeavyAreaCodes.Contains(areaCode);
    }

    /// <summary>
    /// Validates if a phone number format is callable (not special service numbers)
    /// </summary>
    /// <param name="phoneNumber">Phone number to validate</param>
    /// <returns>True if callable, false if special service number</returns>
    public static bool IsCallableNumber(string phoneNumber)
    {
        var normalized = NormalizePhoneNumber(phoneNumber);
        if (normalized.Length < 10)
            return false;

        // Extract NPA (area code) and NXX (exchange)
        var npa = normalized.Length == 11 && normalized[0] == '1' 
            ? normalized.Substring(1, 3) 
            : normalized.Substring(0, 3);
        
        var nxx = normalized.Length == 11 && normalized[0] == '1' 
            ? normalized.Substring(4, 3) 
            : normalized.Substring(3, 3);

        // Check for invalid NPA patterns
        if (npa[0] == '0' || npa[0] == '1') return false; // Invalid area code
        if (npa.Substring(1) == "11") return false; // N11 codes (411, 911, etc.)
        if (npa.Substring(1) == "00") return false; // N00 codes

        // Check for invalid NXX patterns  
        if (nxx[0] == '0' || nxx[0] == '1') return false; // Invalid exchange
        if (nxx.Substring(1) == "11") return false; // X11 codes (directory assistance, etc.)

        // Check for special service numbers
        var specialPrefixes = new[] { "800", "888", "877", "866", "855", "844", "833", "822" }; // Toll-free
        if (specialPrefixes.Contains(npa)) return false;

        var premiumPrefixes = new[] { "900", "976" }; // Premium rate
        if (premiumPrefixes.Contains(npa)) return false;

        return true;
    }

    /// <summary>
    /// Checks if a number appears to be on the Do Not Call registry based on common patterns
    /// Note: This is a heuristic check - real DNC checking requires registry lookups
    /// </summary>
    /// <param name="phoneNumber">Phone number to check</param>
    /// <returns>True if likely on DNC based on patterns</returns>
    public static bool IsLikelyDncNumber(string phoneNumber)
    {
        var normalized = NormalizePhoneNumber(phoneNumber);
        if (normalized.Length < 10)
            return false;

        // Patterns that are commonly on DNC
        // Government/official number patterns
        if (normalized.StartsWith("1202") || normalized.StartsWith("202")) // DC area
            return true;

        // Sequential or patterned numbers often indicate business/government lines
        var lastFour = normalized.Length >= 4 ? normalized.Substring(normalized.Length - 4) : "";
        
        // Check for sequential patterns (1234, 4567, etc.)
        if (IsSequentialDigits(lastFour)) return true;

        // Check for repeated patterns (1111, 2222, etc.)
        if (lastFour.Length == 4 && lastFour.All(c => c == lastFour[0])) return true;

        return false;
    }

    /// <summary>
    /// Helper method to check if digits are sequential
    /// </summary>
    private static bool IsSequentialDigits(string digits)
    {
        if (digits.Length < 3) return false;

        for (int i = 1; i < digits.Length; i++)
        {
            if (digits[i] != digits[i-1] + 1) return false;
        }
        return true;
    }

    /// <summary>
    /// Determines the best time to call based on timezone and call preferences
    /// </summary>
    /// <param name="phoneNumber">Phone number to analyze</param>
    /// <param name="preferredStartHour">Preferred start hour (default 9 AM)</param>
    /// <param name="preferredEndHour">Preferred end hour (default 8 PM)</param>
    /// <returns>Next available call time, or null if number is not callable</returns>
    public static DateTime? GetNextCallTime(string phoneNumber, int preferredStartHour = 9, int preferredEndHour = 20)
    {
        if (!IsValidPhoneNumber(phoneNumber) || !IsCallableNumber(phoneNumber))
            return null;

        var timezone = GetTimezoneFromAreaCode(phoneNumber);
        if (timezone == null)
            return DateTime.Now.AddMinutes(5); // Default to 5 minutes if timezone unknown

        var currentTimeInZone = TimeZoneInfo.ConvertTime(DateTime.Now, timezone);
        
        // If within calling hours, return immediately
        if (IsWithinCallingHours(timezone, preferredStartHour, preferredEndHour))
            return DateTime.Now;

        // Calculate next available time
        var nextCallDate = currentTimeInZone.Date;
        if (currentTimeInZone.Hour >= preferredEndHour)
            nextCallDate = nextCallDate.AddDays(1); // Move to next day

        var nextCallTime = nextCallDate.AddHours(preferredStartHour);
        
        // Convert back to UTC
        return TimeZoneInfo.ConvertTimeToUtc(nextCallTime, timezone);
    }
}
