using Feast.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging; // Don't forget this import
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Feast.Services
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TokenService> _logger;

        public TokenService(IConfiguration configuration, ILogger<TokenService> logger) // Inject the logger here
        {
            _configuration = configuration;
            _logger = logger; // Assign the injected logger
        }

        public string GenerateJwtToken(ApplicationUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings").Get<JwtSettings>();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.UserName), // Keep the username as the NameIdentifier
                new Claim("UserId", user.Id) // Add a custom claim for the user ID (GUID)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings.Issuer,
                audience: jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(20), // Adjust token lifetime as needed
                signingCredentials: creds
            );

            // Logging the key, issuer, audience, and the generated token
            _logger.LogInformation("JWT Signing Key: {SigningKey}", jwtSettings.Secret);
            _logger.LogInformation("JWT Issuer: {Issuer}", jwtSettings.Issuer);
            _logger.LogInformation("JWT Audience: {Audience}", jwtSettings.Audience);

            var generatedToken = new JwtSecurityTokenHandler().WriteToken(token);
            _logger.LogInformation("Generated JWT Token: {Token}", generatedToken);

            return generatedToken;
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}
