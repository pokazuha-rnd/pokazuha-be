using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using Pokazuha.Application.Configuration;
using Pokazuha.Application.DTOs.Auth;
using Pokazuha.Application.Interfaces;


namespace Pokazuha.Application.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly GoogleAuthSettings _googleSettings;

        public GoogleAuthService(IOptions<GoogleAuthSettings> googleSettings)
        {
            _googleSettings = googleSettings.Value;
        }

        public async Task<GoogleUserInfo?> ValidateGoogleTokenAsync(string idToken)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _googleSettings.ClientId }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

                return new GoogleUserInfo
                {
                    GoogleId = payload.Subject,
                    Email = payload.Email,
                    FirstName = payload.GivenName,
                    LastName = payload.FamilyName,
                    Picture = payload.Picture,
                    EmailVerified = payload.EmailVerified
                };
            }
            catch (InvalidJwtException)
            {
                // Token validation failed
                return null;
            }
        }
    }
}
