namespace Feast.Models
{
    public class Recipe
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public Ingredient[] Ingredients { get; set; }
        public string[] Steps { get; set; }
        public int CookTime { get; set; }
        public int? Calories { get; private set; }
        public int? Fat { get; private set; }
        public int? Protein { get; private set; }
        public int? EstimatedCost { get; private set; }
        public string UserId { get; set; }  // Foreign key for ApplicationUser
        public ApplicationUser User { get; set; }  // Navigation property
        public string CardBackgroundColor { get; set; }  // New property for card background color
        public string CardTextColor { get; set; }  // New property for card text color

        // Parameterless constructor for EF Core
        public Recipe() { }

        public Recipe(string name, string image, Ingredient[] ingredients, string[] steps, int cookTime, string userId, string cardBackgroundColor, string cardTextColor)
        {
            Name = name;
            Image = image;
            Ingredients = ingredients;
            Steps = steps;
            CookTime = cookTime;
            UserId = userId; 
            CardBackgroundColor = cardBackgroundColor;
            CardTextColor = cardTextColor;
            DeriveIngredientValues();
        }

        private void DeriveIngredientValues()
        {
            Calories = Ingredients.Sum(ingredient => ingredient.Calories);
            Fat = Ingredients.Sum(ingredient => ingredient.Fat);
            Protein = Ingredients.Sum(ingredient => ingredient.Protein);
            EstimatedCost = Ingredients.Sum(ingredient => ingredient.EstimatedCost);
        }

        public void UpdateIngredients(Ingredient[] ingredients)
        {
            Ingredients = ingredients;
            DeriveIngredientValues();
        }
    }
}