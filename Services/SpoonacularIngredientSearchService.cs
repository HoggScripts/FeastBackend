using Feast.Interfaces;
using Feast.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Feast.Services
{
    public class SpoonacularIngredientSearchService : IIngredientSearchService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly ILogger<SpoonacularIngredientSearchService> _logger;

        public SpoonacularIngredientSearchService(HttpClient httpClient, IConfiguration configuration, ILogger<SpoonacularIngredientSearchService> logger)
        {
            _httpClient = httpClient;
            _apiKey = configuration["FoodAPIs:Spoonacular:ApiKey"];
            _baseUrl = configuration["FoodAPIs:Spoonacular:BaseUrl"];
            _logger = logger;
        }

        public async Task<IEnumerable<Ingredient>> SearchIngredientsAsync(string query)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/search?query={query}&apiKey={_apiKey}");
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<SpoonacularIngredientSearchResponseRoot>(responseContent);

                var ingredients = new List<Ingredient>();

                foreach (var result in apiResponse.Results)
                {
                    var ingredient = new Ingredient
                    {
                        Name = result.Name,
                        Image = $"https://spoonacular.com/cdn/ingredients_100x100/{result.Image}",
                        Description = "", // No description provided in the initial search response
                        Calories = 0,
                        Fat = 0,
                        Protein = 0,
                        Carbohydrates = 0,
                        DietCategory = DietCategory.Any // Default value, needs to be determined based on additional logic
                    };

                    // Fetch detailed information for each ingredient
                    var detailResponse = await _httpClient.GetAsync($"{_baseUrl}/{result.Id}/information?apiKey={_apiKey}");
                    detailResponse.EnsureSuccessStatusCode();
                    var detailContent = await detailResponse.Content.ReadAsStringAsync();
                    var detailApiResponse = JsonConvert.DeserializeObject<SpoonacularIngredientDetailResponse>(detailContent);

                    ingredient.Description = detailApiResponse.Original;
                    // Add logic to map nutritional information if available in the detail response

                    ingredients.Add(ingredient);
                }

                return ingredients;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP Request error occurred while searching for ingredients.");
                throw;
            }
            catch (JsonSerializationException ex)
            {
                _logger.LogError(ex, "JSON Serialization error occurred while processing the API response.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while searching for ingredients.");
                throw;
            }
        }
    }

    public class SpoonacularIngredientSearchResponseRoot
    {
        [JsonProperty("results")]
        public List<SpoonacularIngredientSearchResult> Results { get; set; }
    }

    public class SpoonacularIngredientSearchResult
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }
    }

    public class SpoonacularIngredientDetailResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("original")]
        public string Original { get; set; }

        [JsonProperty("nutrition")]
        public NutritionInfo Nutrition { get; set; }
    }

    public class NutritionInfo
    {
        [JsonProperty("nutrients")]
        public List<Nutrient> Nutrients { get; set; }
    }

    public class Nutrient
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("amount")]
        public double Amount { get; set; }

        [JsonProperty("unit")]
        public string Unit { get; set; }
    }
}

