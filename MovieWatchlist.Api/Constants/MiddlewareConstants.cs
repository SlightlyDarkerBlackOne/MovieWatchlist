namespace MovieWatchlist.Api.Constants;

/// <summary>
/// Constants used by middleware components
/// </summary>
public static class MiddlewareConstants
{
    // HTTP Methods
    public const string HTTP_METHOD_OPTIONS = "OPTIONS";
    
    // Content Types
    public const string CONTENT_TYPE_JSON = "application/json";
    
    // Error Codes
    public const string ERROR_CODE_VALIDATION_ERROR = "VALIDATION_ERROR";
    public const string ERROR_CODE_UNAUTHORIZED = "UNAUTHORIZED";
    public const string ERROR_CODE_BAD_REQUEST = "BAD_REQUEST";
    public const string ERROR_CODE_INTERNAL_SERVER_ERROR = "INTERNAL_SERVER_ERROR";
    
    // Error Messages
    public const string ERROR_MESSAGE_AUTHENTICATION_REQUIRED = "Authentication required";
    public const string ERROR_MESSAGE_INVALID_REQUEST_PARAMETERS = "Invalid request parameters";
    public const string ERROR_MESSAGE_UNEXPECTED_ERROR = "An unexpected error occurred";
    public const string ERROR_MESSAGE_UNHANDLED_EXCEPTION = "An unhandled exception occurred";
    
    // Response Property Names
    public const string PROPERTY_STATUS_CODE = "statusCode";
}

