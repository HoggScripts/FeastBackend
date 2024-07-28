namespace Feast.Models
{
    public class Recipe
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public Ingredient[] Ingredients { get; set; }
        public string Description { get; set; }
        public int CookTime { get; set; }
        public int? Calories { get; private set; }
        public int? Fat { get; private set; }
        public int? Protein { get; private set; }
        public int? EstimatedCost { get; private set; }

        public Recipe(string name, string image, Ingredient[] ingredients, string description, int cookTime)
        {
            Name = name;
            Image = image;
            Ingredients = ingredients;
            Description = description;
            CookTime = cookTime;
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