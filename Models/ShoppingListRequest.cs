namespace Feast.Models
{
    public class ShoppingListRequest
    {
        public List<ShoppingListItem> ThisWeekShoppingList { get; set; } 
        public List<ShoppingListItem> NextWeekShoppingList { get; set; } 
    }

    public class ShoppingListItem
    {
        public string Name { get; set; } 
    }
}