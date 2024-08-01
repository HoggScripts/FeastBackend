using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; 
using System.Security.Claims;
using Feast.Data;
using Feast.Models;

namespace Feast.Controllers
{
    [Authorize]
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
            var userId = User.FindFirstValue("userId"); 

            
            _logger.LogInformation("User ID is: {UserId}", userId);

            var recipe = new Recipe(dto.RecipeName, dto.Image, dto.Ingredients, dto.Steps, dto.CookTime, dto.Servings, userId);

            _context.Recipes.Add(recipe);
            _context.SaveChanges();

            return Ok(recipe);
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

        [HttpPut("{id}")]
        public IActionResult UpdateRecipe(int id, [FromBody] CreateRecipeDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var recipe = _context.Recipes.FirstOrDefault(r => r.Id == id && r.UserId == userId);

            if (recipe == null)
            {
                return NotFound();
            }

            recipe.RecipeName = dto.RecipeName;
            recipe.Image = dto.Image;
            recipe.Ingredients = dto.Ingredients;
            recipe.Steps = dto.Steps;
            recipe.Servings = dto.Servings;
            recipe.CookTime = dto.CookTime;

            recipe.UpdateIngredients(dto.Ingredients);

            _context.Recipes.Update(recipe);
            _context.SaveChanges();

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
