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

        var userId = User.FindFirstValue("UserId");


        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User is not authenticated or UserId is missing." });
        }


        try
        {
            var success = await _googleCalendarService.ScheduleRecipesToGoogleCalendar(userId, request);

   
            if (!success)
            {
                return StatusCode(500, new { message = "Failed to schedule recipes on Google Calendar." });
            }

      
            return Ok(new { message = "All recipes scheduled successfully." });
        }
        catch (Exception ex)
        {
       
           
            return StatusCode(500, new { message = "An error occurred while scheduling recipes.", details = ex.Message });
        }
    }
}