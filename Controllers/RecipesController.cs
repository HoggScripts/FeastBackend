using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; 
using System.Security.Claims;
using Feast.Data;
using Feast.Models;

namespace Feast.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class RecipesController : ControllerBase
    {
        private readonly FeastDbContext _context;
        private readonly ILogger<RecipesController> _logger;

        public RecipesController(FeastDbContext context, ILogger<RecipesController> logger)
        {
            _context = context;
            _logger = logger;
        }

[HttpPost]
public IActionResult CreateRecipe([FromBody] CreateRecipeDto dto)
{
    try
    {

        _logger.LogInformation("Received CreateRecipeDto with Instructions: {@CreateRecipeDto}, Instructions: {@Instructions}", dto, dto.Instructions);

        var userId = User.FindFirstValue("UserId");
        if (userId == null)
        {
            _logger.LogWarning("UserId claim not found in the JWT token.");
            foreach (var claim in User.Claims)
            {
                _logger.LogInformation("JWT Claim: Type = {Type}, Value = {Value}", claim.Type, claim.Value);
            }
        }
        else
        {
            _logger.LogInformation("UserId claim successfully retrieved: {UserId}", userId);
        }

        _logger.LogInformation("User ID is: {UserId}", userId);


        var ingredients = dto.Ingredients.Select(i => 
        {
            var ingredient = new Ingredient
            {
                Id = i.Id,
                Name = i.Name,
                Amount = i.Amount,
                Unit = i.Unit,
                Calories = i.Calories,
                Fat = i.Fat,
                Protein = i.Protein,
                Carbohydrates = i.Carbohydrates,
                EstimatedCost = i.EstimatedCost,
            };

            _logger.LogInformation("Mapped Ingredient: {@Ingredient}", ingredient);
            return ingredient;
        }).ToList();

        _logger.LogInformation("Mapped Ingredients List: {@Ingredients}", ingredients);

        // Create the Recipe object and map instructions to steps
        var recipe = new Recipe(
            dto.RecipeName, 
            dto.Image, 
            ingredients, 
            dto.Instructions, 
            dto.CookTime, 
            dto.Servings, 
            dto.MealType, 
            dto.SpicinessLevel,
            userId
        );

   
        _logger.LogInformation("Received Instructions (as Steps): {@Steps}", dto.Instructions);

        int recipeStepsCount = recipe.Steps.Count;
        if (recipeStepsCount > 0 && recipe.Steps[recipeStepsCount - 1].Trim() == "")
        {
            recipe.Steps.RemoveAt(recipeStepsCount - 1);
        }

 
        _logger.LogInformation("Final Steps after processing: {@Steps}", recipe.Steps);

        _context.Recipes.Add(recipe);
        _context.SaveChanges();

        _logger.LogInformation("Recipe saved successfully with ID: {RecipeId}", recipe.Id);

        return Ok(recipe);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error occurred while creating recipe");
        return BadRequest(new { message = ex.Message, stackTrace = ex.StackTrace });
    }
}


        [HttpGet]
        public IActionResult GetRecipes()
        {
            _logger.LogInformation("Get Recipes endpoint requested...");
            var userId = User.FindFirstValue("UserId");
            _logger.LogInformation("User ID for GETRECIPES is {userID}", userId);
            var recipes = _context.Recipes.Where(r => r.UserId == userId).ToList();

            return Ok(recipes);
        }

        [HttpGet("{id}")]
        public IActionResult GetRecipe(int id)
        {
            var userId = User.FindFirstValue("UserId");
            var recipe = _context.Recipes.FirstOrDefault(r => r.Id == id && r.UserId == userId);

            if (recipe == null)
            {
                return NotFound();
            }

            return Ok(recipe);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteRecipe(int id)
        {
            _logger.LogInformation("Attempting to delete recipe with ID: {RecipeId}", id);

            try
            {
                var userId = User.FindFirstValue("UserId");
                if (userId == null)
                {
                    _logger.LogWarning("User ID could not be found in the claims.");
                    return Unauthorized();
                }

                var recipe = _context.Recipes.FirstOrDefault(r => r.Id == id && r.UserId == userId);

                if (recipe == null)
                {
                    _logger.LogWarning("Recipe with ID: {RecipeId} not found for User ID: {UserId}", id, userId);
                    return NotFound();
                }

                _context.Recipes.Remove(recipe);
                _context.SaveChanges();

                _logger.LogInformation("Recipe with ID: {RecipeId} successfully deleted for User ID: {UserId}", id, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while attempting to delete recipe with ID: {RecipeId}", id);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }





    }
}
    

