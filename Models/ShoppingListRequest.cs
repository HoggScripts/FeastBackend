namespace Feast.Models
{
    public class ShoppingListRequest
    {
        public List<ShoppingListItem> ThisWeekShoppingList { get; set; } // Shopping list for this week
        public List<ShoppingListItem> NextWeekShoppingList { get; set; } // Shopping list for next week
    }

    public class ShoppingListItem
    {
        public string Name { get; set; } // Name of the ingredient
    }
}