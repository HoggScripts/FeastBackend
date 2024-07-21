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
        public string Image { get; set; }
        
        // User Selected Properties
        public double Amount { get; set; }
        public string SelectedUnit { get; set; }
        
        // Detailed Information
        public string Description { get; set; }
        public double? Calories { get; set; }
        public double? Fat { get; set; }
        public double? Protein { get; set; }
        public double? Carbohydrates { get; set; }
        public double? EstimatedCost { get; set; }
        public List<string> PossibleUnits { get; set; }

        // Method to Fetch Possible Units
        public async Task FetchPossibleUnits(HttpClient httpClient, string apiKey, string baseUrl)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/food/ingredients/{Id}/information?amount=1");
            request.Headers.Add("x-rapidapi-key", apiKey);
            request.Headers.Add("x-rapidapi-host", "spoonacular-recipe-food-nutrition-v1.p.rapidapi.com");

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var detailApiResponse = JsonConvert.DeserializeObject<SpoonacularApiResponses.SpoonacularIngredientDetailResponse>(responseContent);

            Description = detailApiResponse.Original;
            PossibleUnits = detailApiResponse.PossibleUnits;
        }

        // Method to Fetch Details and Nutrition
        public async Task FetchDetails(HttpClient httpClient, string apiKey, string baseUrl, double amount, string unit)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/food/ingredients/{Id}/information?amount={amount}&unit={unit}");
            request.Headers.Add("x-rapidapi-key", apiKey);
            request.Headers.Add("x-rapidapi-host", "spoonacular-recipe-food-nutrition-v1.p.rapidapi.com");

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var detailApiResponse = JsonConvert.DeserializeObject<SpoonacularApiResponses.SpoonacularIngredientDetailResponse>(responseContent);

            Description = detailApiResponse.Original;
            EstimatedCost = detailApiResponse.EstimatedCost?.Value;
            PossibleUnits = detailApiResponse.PossibleUnits;

            if (detailApiResponse.Nutrition != null)
            {
                var nutrition = detailApiResponse.Nutrition.Nutrients;
                Calories = GetNutrientValue(nutrition, "Calories");
                Fat = GetNutrientValue(nutrition, "Fat");
                Protein = GetNutrientValue(nutrition, "Protein");
                Carbohydrates = GetNutrientValue(nutrition, "Carbohydrates");
            }
            
            Amount = amount;
            SelectedUnit = unit;
        }

        private double? GetNutrientValue(List<SpoonacularApiResponses.Nutrient> nutrients, string nutrientName)
        {
            var nutrient = nutrients.FirstOrDefault(n => n.Name.Equals(nutrientName, StringComparison.OrdinalIgnoreCase));
            return nutrient?.Amount;
        }
    }
}
