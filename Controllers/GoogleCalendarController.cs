using System.Security.Claims;
using Feast.Models;
using Feast.Services;
using Microsoft.AspNetCore.Mvc;

namespace Feast.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GoogleCalendarController : ControllerBase
{
    private readonly GoogleCalendarService _googleCalendarService;

    public GoogleCalendarController(GoogleCalendarService googleCalendarService)
    {
        _googleCalendarService = googleCalendarService;
    }

    [HttpPost("schedule-recipes")]
    public async Task<IActionResult> ScheduleRecipes([FromBody] ScheduledRecipesRequest request)
    {
        // Get the user ID from the claims in the JWT
        var userId = User.FindFirstValue("UserId");

        // If the user ID is not found, return unauthorized
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User is not authenticated or UserId is missing." });
        }

        // Attempt to schedule the recipes on the user's Google Calendar
        try
        {
            var success = await _googleCalendarService.ScheduleRecipesToGoogleCalendar(userId, request);

            // If scheduling was not successful, return a 500 error
            if (!success)
            {
                return StatusCode(500, new { message = "Failed to schedule recipes on Google Calendar." });
            }

            // If successful, return an OK response
            return Ok(new { message = "All recipes scheduled successfully." });
        }
        catch (Exception ex)
        {
            // Log the exception and return a 500 error with details
           
            return StatusCode(500, new { message = "An error occurred while scheduling recipes.", details = ex.Message });
        }
    }
}