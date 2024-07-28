namespace Feast.Models;

public class SpoonacularAutocompleteResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Image { get; set; }
    public List<string> PossibleUnits { get; set; }
}
