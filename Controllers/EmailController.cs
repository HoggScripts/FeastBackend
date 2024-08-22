using Feast.Models;
using Feast.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using Feast.Data;

namespace Feast.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly EmailService _emailService;
        private readonly ILogger<EmailController> _logger;
        private readonly FeastDbContext _context; // Add DbContext to fetch user data

        public EmailController(EmailService emailService, ILogger<EmailController> logger, FeastDbContext context)
        {
            _emailService = emailService;
            _logger = logger;
            _context = context;
        }

        [HttpPost("send-shopping-list")]
        public async Task<IActionResult> SendShoppingListEmail([FromBody] ShoppingListRequest request)
        {
            var userId = User.FindFirstValue("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User is not authenticated or UserId is missing." });
            }

            // Fetch the user's email from the database using the UserId
            var user = await _context.Users.FindAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.Email))
            {
                return Unauthorized(new { message = "User not found or email is missing." });
            }

            var userEmail = user.Email;

            var subject = "Your Shopping List from Feast";
            var body = BuildShoppingListEmailBody(request, userEmail);

            try
            {
                await _emailService.SendEmailAsync(userEmail, subject, body);
                _logger.LogInformation($"Shopping list email sent successfully to {userEmail}.");
                return Ok(new { message = "Shopping list email sent successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send shopping list email to {userEmail}. Exception: {ex.Message}");
                return StatusCode(500, new { message = "Failed to send shopping list email." });
            }
        }

        private string BuildShoppingListEmailBody(ShoppingListRequest request, string userEmail)
{
    var body = new StringBuilder();

    body.AppendLine("<html>");
    body.AppendLine("<head>");
    body.AppendLine("<style>");
    body.AppendLine("body { font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px; }");
    body.AppendLine(".email-container { max-width: 600px; margin: auto; background-color: #ffffff; padding: 20px; border-radius: 8px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1); }");
    body.AppendLine(".header { background-color: #007BFF; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }");
    body.AppendLine(".content { padding: 20px; color: #333333; }");
    body.AppendLine(".content h2 { color: #007BFF; }");
    body.AppendLine(".footer { padding: 20px; text-align: center; font-size: 12px; color: #777777; }");
    body.AppendLine(".footer a { color: #007BFF; text-decoration: none; }");
    body.AppendLine(".shopping-list { margin: 20px 0; padding: 0; list-style: none; }");
    body.AppendLine(".shopping-list li { background-color: #f9f9f9; margin: 5px 0; padding: 10px; border-radius: 5px; }");
    body.AppendLine(".btn { display: inline-block; padding: 10px 20px; margin-top: 20px; background-color: #007BFF; color: white; text-decoration: none; border-radius: 5px; }");
    body.AppendLine("</style>");
    body.AppendLine("</head>");
    body.AppendLine("<body>");
    body.AppendLine("<div class='email-container'>");
    body.AppendLine("<div class='header'>");
    body.AppendLine($"<h1>Hi {userEmail},</h1>");
    body.AppendLine("</div>");
    body.AppendLine("<div class='content'>");
    body.AppendLine("<h2>Here's your shopping list!</h2>");
    body.AppendLine("<p>Your carefully planned meals are just a shopping trip away. Below is your shopping list for the upcoming week:</p>");

    if (request.ThisWeekShoppingList != null && request.ThisWeekShoppingList.Count > 0)
    {
        body.AppendLine("<h3>This Week:</h3>");
        body.AppendLine("<ul class='shopping-list'>");
        foreach (var item in request.ThisWeekShoppingList)
        {
            body.AppendLine($"<li>{item.Name}</li>");
        }
        body.AppendLine("</ul>");
    }
    else
    {
        body.AppendLine("<h3>This Week:</h3>");
        body.AppendLine("<p>No items scheduled for this week.</p>");
    }

    if (request.NextWeekShoppingList != null && request.NextWeekShoppingList.Count > 0)
    {
        body.AppendLine("<h3>Next Week:</h3>");
        body.AppendLine("<ul class='shopping-list'>");
        foreach (var item in request.NextWeekShoppingList)
        {
            body.AppendLine($"<li>{item.Name}</li>");
        }
        body.AppendLine("</ul>");
    }
    else
    {
        body.AppendLine("<h3>Next Week:</h3>");
        body.AppendLine("<p>No items scheduled for next week.</p>");
    }

    body.AppendLine("<p>Happy cooking!</p>");
    body.AppendLine("</div>");
    body.AppendLine("<div class='footer'>");
    body.AppendLine("<p>&copy; 2024 Feast. All rights reserved.</p>");
    body.AppendLine("<p><a href='#'>Unsubscribe</a></p>");
    body.AppendLine("</div>");
    body.AppendLine("</div>");
    body.AppendLine("</body>");
    body.AppendLine("</html>");

    return body.ToString();
}

    }
}
