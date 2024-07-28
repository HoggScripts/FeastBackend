using Feast.Interfaces;
using Feast.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Feast.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IngredientsController : ControllerBase
    {
        private readonly IIngredientSearchService _ingredientSearchService;
        private readonly ILogger<IngredientsController> _logger;
        private static DateTime _lastRequestTime = DateTime.MinValue;

        public IngredientsController(IIngredientSearchService ingredientSearchService, ILogger<IngredientsController> logger)
        {
            _ingredientSearchService = ingredientSearchService;
            _logger = logger;
        }

        private void LogTimeSinceLastRequest()
        {
            var currentTime = DateTime.UtcNow;
            var timeSinceLastRequest = currentTime - _lastRequestTime;
            _lastRequestTime = currentTime;
            _logger.LogInformation("Time since last request: {TimeSinceLastRequest} ms", timeSinceLastRequest.TotalMilliseconds);
        }


[HttpGet("search")]
public async Task<IActionResult> SearchIngredients(string query)
{
    LogTimeSinceLastRequest();

    try
    {
        _logger.LogInformation("Received search request with query: {Query}", query);

        if (string.IsNullOrWhiteSpace(query))
        {
            _logger.LogWarning("Search query is empty or whitespace.");
            return BadRequest("Query parameter is required.");
        }

        _logger.LogInformation("Initiating ingredient search for query: {Query}", query);
        var ingredients = await _ingredientSearchService.BasicIngredientsSearchAsync(query);

        if (ingredients == null)
        {
            _logger.LogWarning("Received null response from ingredient search service for query: {Query}", query);
            return StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred while processing your request.");
        }

        if (!ingredients.Any())
        {
            _logger.LogInformation("No ingredients found for query: {Query}. Trying autocomplete.", query);
            ingredients = await _ingredientSearchService.AutocompleteIngredientsAsync(query);
        }

        _logger.LogInformation("Found {Count} ingredients for query: {Query}", ingredients.Count(), query);
        return Ok(ingredients);
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "HTTP Request error occurred while searching for ingredients.");
        return StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred while processing your request.");
    }
    catch (JsonSerializationException ex)
    {
        _logger.LogError(ex, "JSON Serialization error occurred while processing the API response.");
        return StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred while processing your request.");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "An unexpected error occurred while searching for ingredients.");
        return StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred while processing your request.");
    }
}



        [HttpGet("{id}/possible-units")]
        public async Task<IActionResult> GetPossibleUnits(int id)
        {
            LogTimeSinceLastRequest();

            try
            {
                _logger.LogInformation("Fetching possible units for ingredient ID: {Id}", id);
                var ingredient = new Ingredient { Id = id };
                await _ingredientSearchService.FetchPossibleUnitsAsync(ingredient);
                _logger.LogInformation("Fetched possible units for ingredient ID: {Id}", id);
                return Ok(ingredient.PossibleUnits);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP Request error occurred while fetching possible units.");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred while processing your request.");
            }
            catch (JsonSerializationException ex)
            {
                _logger.LogError(ex, "JSON Serialization error occurred while processing the API response.");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred while processing your request.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while fetching possible units.");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpGet("{id}/details")]
        public async Task<IActionResult> GetIngredientDetails(int id, [FromQuery] double amount, [FromQuery] string unit)
        {
            LogTimeSinceLastRequest();

            try
            {
                if (amount <= 0 || string.IsNullOrWhiteSpace(unit))
                {
                    _logger.LogWarning("Invalid amount or unit for ingredient details.");
                    return BadRequest("Amount and unit parameters are required and must be valid.");
                }

                _logger.LogInformation("Fetching details for ingredient ID: {Id} with amount: {Amount} and unit: {Unit}", id, amount, unit);
                var ingredient = await _ingredientSearchService.GetIngredientDetailsAsync(id, amount, unit);
                _logger.LogInformation("Fetched details for ingredient ID: {Id}", id);
                return Ok(ingredient);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP Request error occurred while fetching ingredient details.");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred while processing your request.");
            }
            catch (JsonSerializationException ex)
            {
                _logger.LogError(ex, "JSON Serialization error occurred while processing the API response.");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred while processing your request.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while fetching ingredient details.");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred while processing your request.");
            }
        }
    }
}
