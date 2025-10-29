namespace MovieWatchlist.Core.Constants;

public static class ValidationConstants
{
    public static class Rating
    {
        public const int MinValue = 1;
        public const int MaxValue = 10;
        public const int HighRatingThreshold = 7;
        
        public static string InvalidRangeMessage => $"Rating must be between {MinValue} and {MaxValue}";
    }

    public static class Username
    {
        public const int MinLength = 3;
        public const int MaxLength = 50;
        public static string Pattern => $"^[a-zA-Z0-9_-]{{{MinLength},{MaxLength}}}$";
        
        // Dynamic messages that use the constants
        public static string InvalidFormatMessage => 
            $"Username must be {MinLength}-{MaxLength} characters long and contain only letters, numbers, underscores, and hyphens";
        
        public static string InvalidLengthMessage => 
            $"Username must be between {MinLength} and {MaxLength} characters";
    }

    public static class Email
    {
        public const int MaxLength = 100;
        public const string Pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9]([a-zA-Z0-9.-]*[a-zA-Z0-9])?\.[a-zA-Z]{2,}$";
        
        public const string InvalidFormatMessage = "Invalid email format";
        public static string InvalidLengthMessage => $"Email cannot exceed {MaxLength} characters";
    }

    public static class Password
    {
        public const int MinLength = 8;
        public const int MaxLength = 100;
        public const int ResetTokenExpirationHours = 1;
        public static string Pattern => $@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{{{MinLength},{MaxLength}}}$";
        
        public static string InvalidFormatMessage => 
            $"Password must be {MinLength}-{MaxLength} characters with uppercase, lowercase, number, and special character";
    }

    public static class Recommendation
    {
        public const int TopGenresCount = 3;
        public const double MinTmdbRating = 7.0;
        public const int MinVoteCount = 1000;
    }
}

