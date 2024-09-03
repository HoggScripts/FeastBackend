using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Feast.Data;
using Feast.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Feast.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OAuthController : ControllerBase
    {
        private readonly OAuthSettings _oauthSettings;
        private readonly ILogger<OAuthController> _logger;
        private readonly FeastDbContext _context;

        public OAuthController(IOptions<OAuthSettings> oauthSettings, ILogger<OAuthController> logger, FeastDbContext context)
        {
            _oauthSettings = oauthSettings.Value;
            _logger = logger;
            _context = context;
        }

        private string GetUserIdFromJwt(string jwtToken)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(jwtToken);
                var userIdClaim = token.Claims.FirstOrDefault(claim => claim.Type == "UserId")?.Value;

                _logger.LogInformation($"Decoded UserId from JWT: {userIdClaim}");
                return userIdClaim;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decoding JWT token.");
                return null;
            }
        }

        [HttpGet("google-link-status")]
        public async Task<IActionResult> CheckGoogleLinkStatus()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            _logger.LogInformation("User Claims: {@Claims}", claims);

            var userId = User.FindFirstValue("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("UserId not found in claims. User might not be authenticated.");
                return Unauthorized(new { message = "User is not authenticated or UserId is missing." });
            }

            // Check if the user has a Google OAuth token 
            var user = await _context.Users.Include(u => u.GoogleOAuthToken)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.GoogleOAuthToken == null)
            {
                _logger.LogInformation("User has not linked their Google account.");
                return Ok(new { isLinked = false });
            }

            _logger.LogInformation("User has linked their Google account.");
            return Ok(new { isLinked = true });
        }

        [HttpGet("authorize")]
        public IActionResult Authorize(string redirectUrl, string jwt)
        {
            _logger.LogInformation($"Authorize called with RedirectUrl: {redirectUrl} and JWT: {jwt}");

            if (string.IsNullOrEmpty(jwt))
            {
                _logger.LogError("JWT token is null or empty. Cannot proceed with OAuth flow.");
                return Unauthorized(new { message = "User is not authenticated or JWT is missing." });
            }

            if (string.IsNullOrEmpty(redirectUrl))
            {
                _logger.LogError("Redirect URL is null or empty. Cannot proceed with OAuth flow.");
                return BadRequest(new { message = "Redirect URL is missing." });
            }

            var userId = GetUserIdFromJwt(jwt);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("UserId not found in JWT.");
                return Unauthorized(new { message = "UserId not found in JWT." });
            }

            HttpContext.Session.SetString("JWT", jwt);
            HttpContext.Session.SetString("UserId", userId);
            HttpContext.Session.SetString("RedirectUrl", redirectUrl);

            _logger.LogInformation($"JWT and RedirectUrl stored in session. UserId: {userId}");

            var authorizationUrl =
                $"{_oauthSettings.AuthUri}?client_id={_oauthSettings.ClientId}" +
                $"&redirect_uri={_oauthSettings.RedirectUri}" +
                $"&response_type=code" +
                $"&scope=https://www.googleapis.com/auth/calendar" +
                $"&access_type=offline" +
                $"&prompt=consent"; 

            _logger.LogInformation($"Redirecting to: {authorizationUrl}");

            return Redirect(authorizationUrl);
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback(string code)
        {
            _logger.LogInformation("Callback called with authorization code.");

            var jwtToken = HttpContext.Session.GetString("JWT");
            var redirectUrl = HttpContext.Session.GetString("RedirectUrl");
            var userId = HttpContext.Session.GetString("UserId");

            _logger.LogInformation($"Session data - JWT: {jwtToken}, RedirectUrl: {redirectUrl}, UserId: {userId}");

            if (string.IsNullOrEmpty(jwtToken) || string.IsNullOrEmpty(redirectUrl) || string.IsNullOrEmpty(userId))
            {
                _logger.LogError("Session data is missing. Cannot proceed with OAuth callback.");
                return BadRequest(new { message = "Session data missing. Please try again." });
            }

            using (var client = new HttpClient())
            {
                var requestContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("client_id", _oauthSettings.ClientId),
                    new KeyValuePair<string, string>("client_secret", _oauthSettings.ClientSecret),
                    new KeyValuePair<string, string>("redirect_uri", _oauthSettings.RedirectUri),
                    new KeyValuePair<string, string>("grant_type", "authorization_code")
                });

                var tokenResponse = await client.PostAsync(_oauthSettings.TokenUri, requestContent);
                var responseString = await tokenResponse.Content.ReadAsStringAsync();

                _logger.LogInformation($"OAuth Callback Response: {responseString}");

                if (!tokenResponse.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to retrieve tokens. Response: {responseString}");
                    return BadRequest(new { message = "Failed to retrieve tokens", response = responseString });
                }

                var tokenData = JObject.Parse(responseString);
                var accessToken = tokenData["access_token"]?.ToString();
                var refreshToken = tokenData["refresh_token"]?.ToString();
                var expiresInSeconds = tokenData["expires_in"]?.ToString();

                _logger.LogInformation(
                    $"Access Token: {accessToken}, Refresh Token: {refreshToken}, Expires In: {expiresInSeconds}");

                if (accessToken == null || refreshToken == null)
                {
                    _logger.LogError("Invalid token data received.");
                    return BadRequest(new { message = "Invalid token data received." });
                }

                var expiresIn = DateTime.UtcNow.AddSeconds(Convert.ToDouble(expiresInSeconds));

                var user = await _context.Users.Include(u => u.GoogleOAuthToken)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    _logger.LogError($"User not found with UserId: {userId}");
                    return NotFound(new { message = "User not found." });
                }

                if (user.GoogleOAuthToken == null)
                {
                    user.GoogleOAuthToken = new GoogleOAuthToken
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                        AccessTokenExpiry = expiresIn,
                        UserId = user.Id
                    };
                    _context.GoogleOAuthTokens.Add(user.GoogleOAuthToken);
                }
                else
                {
                    user.GoogleOAuthToken.AccessToken = accessToken;
                    user.GoogleOAuthToken.RefreshToken = refreshToken;
                    user.GoogleOAuthToken.AccessTokenExpiry = expiresIn;
                    _context.GoogleOAuthTokens.Update(user.GoogleOAuthToken);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Google OAuth tokens saved successfully. Redirecting back to the original URL.");

                return Redirect(redirectUrl);
            }
        }

  
        private async Task<string> GetValidAccessToken(string userId)
        {
            var user = await _context.Users.Include(u => u.GoogleOAuthToken)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.GoogleOAuthToken == null)
            {
                _logger.LogError("User or Google OAuth token not found.");
                return null;
            }

            if (user.GoogleOAuthToken.AccessTokenExpiry > DateTime.UtcNow)
            {
          
                return user.GoogleOAuthToken.AccessToken;
            }

            _logger.LogInformation("Access token expired, refreshing...");


            using (var client = new HttpClient())
            {
                var requestContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", _oauthSettings.ClientId),
                    new KeyValuePair<string, string>("client_secret", _oauthSettings.ClientSecret),
                    new KeyValuePair<string, string>("refresh_token", user.GoogleOAuthToken.RefreshToken),
                    new KeyValuePair<string, string>("grant_type", "refresh_token")
                });

                var tokenResponse = await client.PostAsync(_oauthSettings.TokenUri, requestContent);
                var responseString = await tokenResponse.Content.ReadAsStringAsync();

                if (!tokenResponse.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to refresh tokens. Response: {responseString}");
                    return null;
                }

                var tokenData = JObject.Parse(responseString);
                var newAccessToken = tokenData["access_token"]?.ToString();
                var expiresInSeconds = tokenData["expires_in"]?.ToString();

                _logger.LogInformation($"New Access Token: {newAccessToken}, Expires In: {expiresInSeconds}");

                if (newAccessToken == null)
                {
                    _logger.LogError("Invalid token data received.");
                    return null;
                }

                var expiresIn = DateTime.UtcNow.AddSeconds(Convert.ToDouble(expiresInSeconds));

    
                user.GoogleOAuthToken.AccessToken = newAccessToken;
                user.GoogleOAuthToken.AccessTokenExpiry = expiresIn;
                _context.GoogleOAuthTokens.Update(user.GoogleOAuthToken);

                await _context.SaveChangesAsync();

                return newAccessToken;
            }
        }
    }
}
