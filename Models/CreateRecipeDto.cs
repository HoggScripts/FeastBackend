namespace Feast.Models
{
    public class CreateRecipeDto
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public Ingredient[] Ingredients { get; set; }
        public string[] Steps { get; set; }
        public int CookTime { get; set; }
        public string CardBackgroundColor { get; set; }  // New property for card background color
        public string CardTextColor { get; set; }       // New property for card text color
    }
}