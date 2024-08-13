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
                // Log the entire incoming DTO
                _logger.LogInformation("Received CreateRecipeDto: {@CreateRecipeDto}", dto);

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

                // Convert CreateIngredientDto to Ingredient and log each ingredient
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

                // Create the Recipe object and log it
                var recipe = new Recipe(
                    dto.RecipeName, 
                    dto.Image, 
                    ingredients, 
                    dto.Steps, 
                    dto.CookTime, 
                    dto.Servings, 
                    dto.MealType, 
                    dto.SpicinessLevel,
                    userId
                );

                int recipeStepsCount = recipe.Steps.Count;
                if (recipeStepsCount > 0 && recipe.Steps[recipeStepsCount - 1].Trim() == "")
                {
                    recipe.Steps.RemoveAt(recipeStepsCount - 1);
                }
                _logger.LogInformation("Created Recipe object: {@Recipe}", recipe);

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

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var recipe = _context.Recipes.FirstOrDefault(r => r.Id == id && r.UserId == userId);

            if (recipe == null)
            {
                return NotFound();
            }

            _context.Recipes.Remove(recipe);
            _context.SaveChanges();

            return NoContent();
        }
        [HttpGet("search/spiciness")]
        public IActionResult SearchBySpicinessLevel(int level)
        {
            var userId = User.FindFirstValue("UserId");
            var recipes = _context.Recipes
                .Where(r => r.UserId == userId && r.SpicinessLevel >= level)
                .ToList();

            return Ok(recipes);
        }

        [HttpGet("search/mealtype")]
        public IActionResult SearchByMealType(string mealType)
        {
            var userId = User.FindFirstValue("UserId");
            var recipes = _context.Recipes
                .Where(r => r.UserId == userId && r.MealType.ToLower() == mealType.ToLower())
                .ToList();

            return Ok(recipes);
        }

        [HttpGet("search/calories")]
        public IActionResult SearchByCaloriesRange(int min, int max)
        {
            var userId = User.FindFirstValue("UserId");
            var recipes = _context.Recipes
                .Where(r => r.UserId == userId && r.Calories >= min && r.Calories <= max)
                .ToList();

            return Ok(recipes);
        }

        [HttpGet("search/cooktime")]
        public IActionResult SearchByCookTime(int max)
        {
            var userId = User.FindFirstValue("UserId");
            var recipes = _context.Recipes
                .Where(r => r.UserId == userId && r.CookTime <= max)
                .ToList();

            return Ok(recipes);
        }

        [HttpGet("search")]
        public IActionResult SearchByMultipleCriteria(string? mealType, int? minSpiciness, int? maxCalories, int? maxCookTime)
        {
            var userId = User.FindFirstValue("UserId");
            var query = _context.Recipes.AsQueryable();

            if (!string.IsNullOrEmpty(mealType))
            {
                query = query.Where(r => r.UserId == userId && r.MealType.ToLower() == mealType.ToLower());
            }

            if (minSpiciness.HasValue)
            {
                query = query.Where(r => r.SpicinessLevel >= minSpiciness.Value);
            }

            if (maxCalories.HasValue)
            {
                query = query.Where(r => r.Calories <= maxCalories.Value);
            }

            if (maxCookTime.HasValue)
            {
                query = query.Where(r => r.CookTime <= maxCookTime.Value);
            }

            var recipes = query.ToList();
            return Ok(recipes);
        }

        [HttpGet("search/protein")]
        public IActionResult SearchByProtein(int minProtein)
        {
            var userId = User.FindFirstValue("UserId");
            var recipes = _context.Recipes
                .Where(r => r.UserId == userId && r.Protein >= minProtein)
                .ToList();

            return Ok(recipes);
        }

        [HttpGet("search/cost")]
        public IActionResult SearchByCostRange(int minCost, int maxCost)
        {
            var userId = User.FindFirstValue("UserId");
            var recipes = _context.Recipes
                .Where(r => r.UserId == userId && r.EstimatedCost >= minCost && r.EstimatedCost <= maxCost)
                .ToList();

            return Ok(recipes);
        }
    }
}
    

