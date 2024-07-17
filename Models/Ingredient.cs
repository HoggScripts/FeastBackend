namespace Feast.Models;

public class Ingredient
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Image { get; set; }
    public string Description { get; set; }
    public double Quantity { get; set; }
    public string Unit { get; set; }
    public int Calories { get; set; }
    public int Fat { get; set; }
    public int Protein { get; set; }
    public int Carbohydrates { get; set; }
    public DietCategory DietCategory { get; set; }
}



public enum DietCategory
{
    Vegan,
    Vegetarian,
    GlutenFree,
    DairyFree,
    Any
}

