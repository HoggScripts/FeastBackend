using Feast.ApiResponses;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace Feast.Models
{
    public class Ingredient
    {
        // Basic Information
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Image { get; set; }
        
        // User selected
        public double Amount { get; set; }
        public string Unit { get; set; }
        
        // Detailed info
        public int? Calories { get; set; }
        public int? Fat { get; set; }
        public int? Protein { get; set; }
        public int? Carbohydrates { get; set; }
        public int? EstimatedCost { get; set; }
        public List<string>? PossibleUnits { get; set; }
        
        public async Task FetchPossibleUnits(HttpClient httpClient, string apiKey, string baseUrl)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/food/ingredients/{Id}/information?amount=1");
            request.Headers.Add("x-rapidapi-key", apiKey);
            request.Headers.Add("x-rapidapi-host", "spoonacular-recipe-food-nutrition-v1.p.rapidapi.com");

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var detailApiResponse = JsonConvert.DeserializeObject<SpoonacularApiResponses.SpoonacularIngredientDetailResponse>(responseContent);

            PossibleUnits = detailApiResponse.PossibleUnits;
        }
        
        public async Task FetchDetails(HttpClient httpClient, string apiKey, string baseUrl, double amount, string unit)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/food/ingredients/{Id}/information?amount={amount}&unit={unit}");
            request.Headers.Add("x-rapidapi-key", apiKey);
            request.Headers.Add("x-rapidapi-host", "spoonacular-recipe-food-nutrition-v1.p.rapidapi.com");

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var detailApiResponse = JsonConvert.DeserializeObject<SpoonacularApiResponses.SpoonacularIngredientDetailResponse>(responseContent);

            EstimatedCost = (int?)detailApiResponse.EstimatedCost?.Value;
            PossibleUnits = detailApiResponse.PossibleUnits;

            if (detailApiResponse.Nutrition != null)
            {
                var nutrition = detailApiResponse.Nutrition.Nutrients;
                Calories = (int?)GetNutrientValue(nutrition, "Calories");
                Fat = (int?)GetNutrientValue(nutrition, "Fat");
                Protein = (int?)GetNutrientValue(nutrition, "Protein");
                Carbohydrates = (int?)GetNutrientValue(nutrition, "Carbohydrates");
            }
            
            Amount = amount;
            Unit = unit;
        }

        private double? GetNutrientValue(List<SpoonacularApiResponses.Nutrient> nutrients, string nutrientName)
        {
            var nutrient = nutrients.FirstOrDefault(n => n.Name.Equals(nutrientName, StringComparison.OrdinalIgnoreCase));
            return nutrient?.Amount;
        }

        public async Task FetchDetailsAndCost(HttpClient httpClient, string apiKey, string baseUrl, double amount, string unit)
        {
        
            await FetchDetails(httpClient, apiKey, baseUrl, amount, unit);
            
            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/food/ingredients/{Id}/information?amount={amount}&unit={unit}");
            request.Headers.Add("x-rapidapi-key", apiKey);
            request.Headers.Add("x-rapidapi-host", "spoonacular-recipe-food-nutrition-v1.p.rapidapi.com");

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var detailApiResponse = JsonConvert.DeserializeObject<SpoonacularApiResponses.SpoonacularIngredientDetailResponse>(responseContent);

            EstimatedCost = (int?)detailApiResponse.EstimatedCost?.Value;
        }
    }
}
