namespace Feast.Models;

public class ScheduledRecipesRequest
{
    public List<ScheduledRecipe> ThisWeekRecipes { get; set; }
    public List<ScheduledRecipe> NextWeekRecipes { get; set; }
    public string TimeZone { get; set; }
}


public class ScheduledRecipe
{
    public string RecipeName { get; set; } 
    public DateTime Date { get; set; } 
    public string MealType { get; set; }
}
