using System.Net.Http.Headers;
using System.Text;
using Feast.Data;
using Feast.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Feast.Services;

public class GoogleCalendarService
{
    private readonly FeastDbContext _context;
    private readonly OAuthSettings _oauthSettings;
    private readonly ILogger<GoogleCalendarService> _logger;

    public GoogleCalendarService(FeastDbContext context, IOptions<OAuthSettings> oauthSettings, ILogger<GoogleCalendarService> logger)
    {
        _context = context;
        _oauthSettings = oauthSettings.Value;
        _logger = logger;
    }

    public async Task<string> GetValidAccessToken(string userId)
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

  
    public async Task<bool> AddEventToGoogleCalendar(string userId, CalendarEvent eventModel)
    {
        var accessToken = await GetValidAccessToken(userId);

        if (accessToken == null)
        {
            _logger.LogError("Could not retrieve a valid access token.");
            return false;
        }

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

         
            var utcStartTime = eventModel.StartTime.ToUniversalTime();
            var utcEndTime = eventModel.EndTime.ToUniversalTime();

         
            _logger.LogInformation("Adding event to Google Calendar:");
            _logger.LogInformation($"Title: {eventModel.Title}");
            _logger.LogInformation($"Start Time (Local): {eventModel.StartTime}");
            _logger.LogInformation($"End Time (Local): {eventModel.EndTime}");
            _logger.LogInformation($"Start Time (UTC): {utcStartTime}");
            _logger.LogInformation($"End Time (UTC): {utcEndTime}");
            _logger.LogInformation($"TimeZone: {eventModel.TimeZone}");

            var calendarEvent = new
            {
                summary = eventModel.Title,
                description = eventModel.Description,
                start = new
                {
                    dateTime = utcStartTime.ToString("o"), 
                    timeZone = eventModel.TimeZone 
                },
                end = new
                {
                    dateTime = utcEndTime.ToString("o"), // Send as UTC
                    timeZone = eventModel.TimeZone 
                }
            };

            var content = new StringContent(JsonConvert.SerializeObject(calendarEvent), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://www.googleapis.com/calendar/v3/calendars/primary/events", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to add event to Google Calendar. Response: {await response.Content.ReadAsStringAsync()}");
                return false;
            }

            _logger.LogInformation("Event added to Google Calendar successfully.");
            return true;
        }
    }

   // Schedule recipes for google
    public async Task<bool> ScheduleRecipesToGoogleCalendar(string userId, ScheduledRecipesRequest request)
    {
        var accessToken = await GetValidAccessToken(userId);

        if (accessToken == null)
        {
            _logger.LogError("Could not retrieve a valid access token.");
            return false;
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            _logger.LogError("User not found.");
            return false;
        }

        // Combine the recipes from this week and next week
        var allRecipes = request.ThisWeekRecipes.Concat(request.NextWeekRecipes).ToList();

        foreach (var scheduledRecipe in allRecipes)
        {
            // calculate the meal time based on meal type
            TimeSpan mealTime = scheduledRecipe.MealType switch
            {
                "Breakfast" => user.BreakfastTime,
                "Lunch" => user.LunchTime,
                "Dinner" => user.DinnerTime,
                _ => throw new ArgumentException("Invalid meal type")
            };

            // Combine the scheduled date and meal time
            var scheduledDate = scheduledRecipe.Date.Date;
            var startTime = DateTime.SpecifyKind(scheduledDate + mealTime, DateTimeKind.Local); // Specify as Local time
            var utcStartTime = startTime.ToUniversalTime(); // Convert to UTC

            
            _logger.LogInformation($"Scheduling Recipe: {scheduledRecipe.RecipeName}");
            _logger.LogInformation($"Scheduled Date: {scheduledDate}, Meal Time: {mealTime}");
            _logger.LogInformation($"Start Time (Local): {startTime}");
            _logger.LogInformation($"Start Time (UTC): {utcStartTime}");

            var cookTime = GetCookTimeForRecipe(scheduledRecipe.RecipeName);
            var endTime = utcStartTime.AddMinutes(cookTime);

            _logger.LogInformation($"End Time (UTC): {endTime}, Cook Time: {cookTime} minutes");

            var calendarEvent = new CalendarEvent
            {
                Title = scheduledRecipe.RecipeName,
                StartTime = startTime,
                EndTime = endTime,
                Description = $"Scheduled meal: {scheduledRecipe.RecipeName}",
                TimeZone = request.TimeZone
            };

            var eventAdded = await AddEventToGoogleCalendar(userId, calendarEvent);

            if (!eventAdded)
            {
                _logger.LogError($"Failed to add {scheduledRecipe.RecipeName} to Google Calendar.");
                return false;
            }
        }

        return true;
    }


    private int GetCookTimeForRecipe(string recipeName)
    {
        var recipe = _context.Recipes.FirstOrDefault(r => r.RecipeName == recipeName);
        return recipe?.CookTime ?? 1; // Default to 1 minutes 
    }
}
