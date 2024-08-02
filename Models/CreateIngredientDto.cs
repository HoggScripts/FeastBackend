namespace Feast.Models;

public class CreateIngredientDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public double Amount { get; set; }
    public string Unit { get; set; }
    public int? Calories { get; set; }
    public int? Fat { get; set; }
    public int? Protein { get; set; }
    public int? Carbohydrates { get; set; }
    public int? EstimatedCost { get; set; }
}