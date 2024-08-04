using Feast.Models;
using System.Collections.Generic;
using System.Linq;

public class Recipe
{
    public int Id { get; set; }
    public string RecipeName { get; set; }
    public string? Image { get; set; }
    public List<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
    public List<string> Steps { get; set; } = new List<string>();
    public int CookTime { get; set; }
    public int Servings { get; set; }
    public string MealType { get; set; } // Changed to string
    public int SpicinessLevel { get; set; }
    public int? Calories { get; private set; }
    public int? Fat { get; private set; }
    public int? Protein { get; private set; }
    public int? Carbohydrates { get; private set; }
    public int? EstimatedCost { get; private set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }

    public Recipe() { }

    public Recipe(string recipeName, string? image, List<Ingredient> ingredients, List<string> steps, int cookTime, int servings, string mealType, int spicinessLevel, string userId)
    {
        RecipeName = recipeName;
        Image = image;
        Ingredients = ingredients;
        Steps = steps;
        CookTime = cookTime;
        Servings = servings;
        MealType = mealType;
        SpicinessLevel = spicinessLevel;
        UserId = userId;
        DeriveIngredientValues();
    }

    private void DeriveIngredientValues()
    {
        Calories = Ingredients.Sum(ingredient => ingredient.Calories);
        Fat = Ingredients.Sum(ingredient => ingredient.Fat);
        Protein = Ingredients.Sum(ingredient => ingredient.Protein);
        Carbohydrates = Ingredients.Sum(ingredient => ingredient.Carbohydrates);
        EstimatedCost = Ingredients.Sum(ingredient => ingredient.EstimatedCost);
    }

    public void UpdateIngredients(List<Ingredient> ingredients)
    {
        Ingredients = ingredients;
        DeriveIngredientValues();
    }
}