using Feast.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Feast.Interfaces
{
    public interface IIngredientSearchService
    {
        Task<IEnumerable<Ingredient>> BasicIngredientsSearchAsync(string query);
        Task FetchPossibleUnitsAsync(Ingredient ingredient);
        Task<Ingredient> GetIngredientDetailsAsync(int id, double amount, string unit);
    }
}