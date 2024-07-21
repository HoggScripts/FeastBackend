using Newtonsoft.Json;

namespace Feast.ApiResponses;

public class SpoonacularApiResponses
{
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

        [JsonProperty("originalName")]
        public string OriginalName { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("nutrition")]
        public NutritionInfo Nutrition { get; set; }

        [JsonProperty("estimatedCost")]
        public EstimatedCost EstimatedCost { get; set; }

        [JsonProperty("possibleUnits")]
        public List<string> PossibleUnits { get; set; }
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

    public class EstimatedCost
    {
        [JsonProperty("value")]
        public double Value { get; set; }

        [JsonProperty("unit")]
        public string Unit { get; set; }
    }

    public class AmountResponse
    {
        [JsonProperty("amount")]
        public double Amount { get; set; }

        [JsonProperty("unit")]
        public string Unit { get; set; }
    }

    public class ConvertResponse
    {
        [JsonProperty("targetAmount")]
        public double TargetAmount { get; set; }

        [JsonProperty("targetUnit")]
        public string TargetUnit { get; set; }
    }
}
