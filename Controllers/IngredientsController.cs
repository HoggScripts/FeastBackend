using Feast.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using Newtonsoft.Json;

namespace Feast.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IngredientsController : ControllerBase
    {
        private readonly IIngredientSearchService _ingredientSearchService;
        private readonly ILogger<IngredientsController> _logger;

        public IngredientsController(IIngredientSearchService ingredientSearchService, ILogger<IngredientsController> logger)
        {
            _ingredientSearchService = ingredientSearchService;
            _logger = logger;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchIngredients(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    _logger.LogWarning("Search query is empty or whitespace.");
                    return BadRequest("Query parameter is required.");
                }

                _logger.LogInformation("Searching for ingredients with query: {Query}", query);
                var ingredients = await _ingredientSearchService.SearchIngredientsAsync(query);
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
    }
}
