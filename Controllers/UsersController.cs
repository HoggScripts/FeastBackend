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

        public UsersController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            EmailService emailService, ILogger<UsersController> logger, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _logger = logger;
            _configuration = configuration;
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
        LastName = model.LastName
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
                var token = GenerateJwtToken(user);
                return Ok(new { Token = token });
            }

            return Unauthorized("Invalid login attempt.");
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings").Get<JwtSettings>();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings.Issuer,
                audience: jwtSettings.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
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
                return BadRequest("User not found.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = HttpUtility.UrlEncode(token);
            var resetLink = Url.Action(nameof(ConfirmResetPassword), "Users", new { token = encodedToken, email = user.Email }, Request.Scheme);

            // Log the token for debugging
            _logger.LogInformation($"Password reset token for {model.Email}: {encodedToken}");

            await _emailService.SendEmailAsync(user.Email, "Reset your password", resetLink);

            return Ok("Password reset link has been sent to your email.");
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
