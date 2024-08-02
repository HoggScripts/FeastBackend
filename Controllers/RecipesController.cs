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
        
// UserId instead?
        var userId = User.FindFirstValue("UserId");
        if (userId == null)
        {
            _logger.LogWarning("UserId claim not found in the JWT token.");
            // Log all claims in the JWT
            foreach (var claim in User.Claims)
            {
                _logger.LogInformation("JWT Claim: Type = {Type}, Value = {Value}", claim.Type, claim.Value);
            }
        }
        else
        {
            _logger.LogInformation("UserId claim successfully retrieved: {UserId}", userId);
        }



        // Log the user ID
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
                // Image and PossibleUnits are not mapped
            };

            // Log the mapped ingredient
            _logger.LogInformation("Mapped Ingredient: {@Ingredient}", ingredient);
            return ingredient;
        }).ToList();

        // Log the list of ingredients
        _logger.LogInformation("Mapped Ingredients List: {@Ingredients}", ingredients);

        // Create the Recipe object and log it
        var recipe = new Recipe(dto.RecipeName, dto.Image, ingredients, dto.Steps, dto.CookTime, dto.Servings, userId);
        _logger.LogInformation("Created Recipe object: {@Recipe}", recipe);

        // Add the recipe to the context and save changes
        _context.Recipes.Add(recipe);
        _context.SaveChanges();

        // Log successful save
        _logger.LogInformation("Recipe saved successfully with ID: {RecipeId}", recipe.Id);

        return Ok(recipe);
    }
    catch (Exception ex)
    {
        // Log the exception with its stack trace
        _logger.LogError(ex, "Error occurred while creating recipe");
        return BadRequest(new { message = ex.Message, stackTrace = ex.StackTrace });
    }
}


        [HttpGet]
        public IActionResult GetRecipes()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var recipes = _context.Recipes.Where(r => r.UserId == userId).ToList();

            return Ok(recipes);
        }

        [HttpGet("{id}")]
        public IActionResult GetRecipe(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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
    }
}
