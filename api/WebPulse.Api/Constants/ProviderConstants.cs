namespace WebPulse.Api.Constants;

public static class ProviderConstants
{
    public static class Reddit
    {
        public const string BaseUrl = "https://www.reddit.com";
        public const string ApiEndpoint = "/r/all/new.json";
        public const string UserAgent = "WebPulse/1.0";
        public const string SourcePrefix = "Reddit/r/";
        public const string PostUrlFormat = "https://reddit.com{0}";
        
        public const int DefaultLimit = 25;
        public const int PollingIntervalSeconds = 5;
    }
    
    public static class RSS
    {
        public const string SourcePrefix = "RSS/";
        public const int PollingIntervalSeconds = 10;
    }
    
    public static class YouTube
    {
        public const string SourcePrefix = "YouTube/";
        public const int PollingIntervalSeconds = 3;
    }
    
    public static class TestGenerator
    {
        public const string SourceName = "TestGenerator";
        public const int PollingIntervalSeconds = 2;
    }
    
    public static class SignalR
    {
        public const string HubPath = "/pulseHub";
        public const string GroupName = "AllPulses";
        public const string ReceiveMethod = "ReceivePulse";
    }
    
    public static class UI
    {
        public const string PositiveColor = "#00ff00";
        public const string NegativeColor = "#ff0000";
        public const string NeutralColor = "#ffff00";
        public const float PositiveThreshold = 0.3f;
        public const float NegativeThreshold = -0.3f;
    }
    
    public static class CORS
    {
        public const string PolicyName = "AllowAngular";
        public const string AngularOrigin = "http://localhost:4200";
    }
}
