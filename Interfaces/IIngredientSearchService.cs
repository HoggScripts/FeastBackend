using Feast.Models;

namespace Feast.Interfaces;

public interface IIngredientSearchService
{
    Task<IEnumerable<Ingredient>> SearchIngredientsAsync(string query);
}
