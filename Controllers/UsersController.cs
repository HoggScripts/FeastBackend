using Feast.Models;
using Feast.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Web;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Feast.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly EmailService _emailService;
        private readonly ILogger<UsersController> _logger;
        private readonly IConfiguration _configuration;
        private readonly TokenService _tokenService;

        public UsersController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            EmailService emailService, ILogger<UsersController> logger, IConfiguration configuration, TokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _logger = logger;
            _configuration = configuration;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return BadRequest("User already exists.");
            }

            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                RefreshToken = _tokenService.GenerateRefreshToken(),
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7) // Set an initial expiry date
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = HttpUtility.UrlEncode(token);
            var confirmationLink = Url.Action(nameof(ConfirmEmail), "Users", new { token = encodedToken, email = user.Email }, Request.Scheme);

            // Log the token for debugging
            _logger.LogInformation($"Email confirmation token for {model.Email}: {encodedToken}");

            await _emailService.SendEmailAsync(user.Email, "Confirm your email", confirmationLink);

            return Ok("Registration successful. Please check your email to confirm your account.");
        }


        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            _logger.LogInformation($"Received request to confirm email with token: {token} and email: {email}");

            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Token or email is missing.");
                return BadRequest("Token and email are required.");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning($"No user found with email: {email}");
                return BadRequest("Invalid request.");
            }

            // Log the token for debugging
            _logger.LogInformation($"User found with email: {email}. Confirming email with token: {token}");

            var decodedToken = HttpUtility.UrlDecode(token);
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
            if (result.Succeeded)
            {
                _logger.LogInformation($"Email confirmed successfully for {email}");
                return Ok("Email confirmed successfully.");
            }

            _logger.LogWarning($"Failed to confirm email for {email}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            return BadRequest("Email could not be confirmed.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null && !await _userManager.IsEmailConfirmedAsync(user))
            {
                return Unauthorized("Email is not confirmed.");
            }

            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);
            if (result.Succeeded)
            {
                var accessToken = _tokenService.GenerateJwtToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken();

                // Save refresh token in the database or other persistent storage
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Ensure this is in UTC
                await _userManager.UpdateAsync(user);

                // Log the refresh token being stored
                _logger.LogInformation($"Storing refresh token: {refreshToken} for user: {user.UserName}");

                // Set refresh token as an HTTP-only cookie
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false, // should be true in production
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(7) // Ensure this is in UTC
                };
                Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

                // Log setting the cookie
                _logger.LogInformation($"Set cookie refreshToken with value: {refreshToken}");

                return Ok(new { accessToken });
            }

            return Unauthorized("Invalid login attempt.");
        }
        

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            // Log the incoming request
            _logger.LogInformation("Received request to refresh token");

            var refreshToken = Request.Cookies["refreshToken"];
    
            // Log the received refresh token
            _logger.LogInformation($"Received refresh token: {refreshToken}");

            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogWarning("Refresh token is missing.");
                return Unauthorized("Refresh token is missing.");
            }

            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.RefreshToken == refreshToken);
    
            // Log the user lookup result
            if (user == null)
            {
                _logger.LogWarning("Invalid refresh token.");
                return Unauthorized("Invalid refresh token.");
            }

            // Log the token expiry time
            _logger.LogInformation($"Refresh token expiry time: {user.RefreshTokenExpiryTime}, Current time: {DateTime.UtcNow}");

            if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                _logger.LogWarning("Expired refresh token.");
                return Unauthorized("Expired refresh token.");
            }

            var accessToken = _tokenService.GenerateJwtToken(user);
    
            // Log the new access token
            _logger.LogInformation($"Generated new access token: {accessToken}");

            return Ok(new { accessToken });
        }


        [HttpGet("current-user")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var username = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation($"Username from claims: {username}");

            if (string.IsNullOrEmpty(username))
            {
                _logger.LogWarning("Unauthorized access attempt to /current-user endpoint. No Username found in claims.");
                return Unauthorized();
            }

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                _logger.LogWarning($"Unauthorized access to /current-user: No user found with Username {username}.");
                return Unauthorized();
            }

            _logger.LogInformation($"User found: {user.UserName}");

            var userModel = new UserModel
            {
                Username = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            };

            return Ok(userModel);
        }
        
        [HttpGet("protected-endpoint")]
        [Authorize]
        public IActionResult ProtectedEndpoint()
        {
            return Ok(new { message = "This is protected data." });
        }
        
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // To prevent account enumeration attacks, consider returning Ok() even if the user is not found.
                return Ok("If an account with the email exists, a password reset link has been sent.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = HttpUtility.UrlEncode(token);
            var resetLink = Url.Action(nameof(ConfirmResetPassword), "Users", new { token = encodedToken, email = model.Email }, Request.Scheme);

            // Send the reset link via email
            await _emailService.SendEmailAsync(model.Email, "Reset Password", $"Please reset your password by clicking here: {resetLink}");

            return Ok("If an account with the email exists, a password reset link has been sent.");
        }


        [HttpPost("confirm-reset-password")]
        public async Task<IActionResult> ConfirmResetPassword([FromBody] ConfirmResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            // Log the token for debugging
            _logger.LogInformation($"Confirming password reset for {model.Email} with token: {model.Token}");

            var decodedToken = HttpUtility.UrlDecode(model.Token);
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);
            if (result.Succeeded)
            {
                return Ok("Password has been reset successfully.");
            }

            // Log the errors for debugging
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return BadRequest($"Error occurred while resetting the password: {errors}");
        }
    }
}
