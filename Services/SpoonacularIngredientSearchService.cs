using Feast.Interfaces;
using Feast.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Feast.ApiResponses;

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

        public async Task<IEnumerable<Ingredient>> BasicIngredientsSearchAsync(string query)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/food/ingredients/search?query={query}");
            request.Headers.Add("x-rapidapi-key", _apiKey);
            request.Headers.Add("x-rapidapi-host", "spoonacular-recipe-food-nutrition-v1.p.rapidapi.com");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<SpoonacularApiResponses.SpoonacularIngredientSearchResponseRoot>(responseContent);

            var ingredients = new List<Ingredient>();

            foreach (var result in apiResponse.Results)
            {
                var ingredient = new Ingredient
                {
                    Id = result.Id,
                    Name = result.Name,
                    Image = $"https://spoonacular.com/cdn/ingredients_100x100/{result.Image}"
                };

                ingredients.Add(ingredient);
            }

            return ingredients;
        }

        public async Task FetchPossibleUnitsAsync(Ingredient ingredient)
        {
            await ingredient.FetchPossibleUnits(_httpClient, _apiKey, _baseUrl);
        }

        public async Task<Ingredient> GetIngredientDetailsAsync(int id, double amount, string unit)
        {
            var ingredient = new Ingredient { Id = id };
            await ingredient.FetchDetails(_httpClient, _apiKey, _baseUrl, amount, unit);
            return ingredient;
        }
    }
}
