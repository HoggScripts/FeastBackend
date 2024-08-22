namespace Feast.Models;

public class ScheduledRecipesRequest
{
    public List<ScheduledRecipe> ThisWeekRecipes { get; set; }
    public List<ScheduledRecipe> NextWeekRecipes { get; set; }
    public string TimeZone { get; set; }
}


public class ScheduledRecipe
{
    public string RecipeName { get; set; } // Name of the recipe
    public DateTime Date { get; set; } // Date the recipe is scheduled
    public string MealType { get; set; }
}
