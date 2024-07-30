using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public RecipesController(FeastDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult CreateRecipe([FromBody] CreateRecipeDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get user ID from the claims
            var recipe = new Recipe(dto.Name, dto.Image, dto.Ingredients, dto.Steps, dto.CookTime, userId, dto.CardBackgroundColor, dto.CardTextColor);

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

            recipe.Name = dto.Name;
            recipe.Image = dto.Image;
            recipe.Ingredients = dto.Ingredients;
            recipe.Steps = dto.Steps;
            recipe.CookTime = dto.CookTime;
            recipe.CardBackgroundColor = dto.CardBackgroundColor;
            recipe.CardTextColor = dto.CardTextColor;
            recipe.UpdateIngredients(dto.Ingredients);

            _context.Recipes.Update(recipe);
            _context.SaveChanges();

            return Ok(recipe);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteRecipe(int id)
        {
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

