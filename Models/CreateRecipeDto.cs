namespace Feast.Models
{
    public class CreateRecipeDto
    {
        public string RecipeName { get; set; }
        public string? Image { get; set; } 
        public List<CreateIngredientDto> Ingredients { get; set; } = new List<CreateIngredientDto>();  
        public List<string> Instructions { get; set; } = new List<string>();  
        public int Servings { get; set; }
        public int CookTime { get; set; }
        public string MealType { get; set; }
        public int SpicinessLevel { get; set; }
    }
}