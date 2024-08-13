namespace Feast.Models
{
    public class OAuthSettings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectUri { get; set; }
        public string AuthUri { get; set; }
        public string TokenUri { get; set; }
        public string AuthProviderCertUrl { get; set; }
        public string JavascriptOrigins { get; set; } // Specify allowed JavaScript origins
        public string FrontendRedirectUri { get; set; } // Redirect back to the frontend after OAuth
    }
}