namespace Pokazuha.Application.Configuration
{
    public class GoogleAuthSettings
    {
        public const string SectionName = "GoogleAuth";

        public string ClientId { get; set; } = null!;
        public string ClientSecret { get; set; } = null!;
    }
}
