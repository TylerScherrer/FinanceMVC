using BudgetTracker.Interfaces;
using BudgetTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public IActionResult Create(int budgetId)
        {
            var category = new Category
            {
                BudgetId = budgetId // Pre-fill the BudgetId
            };

            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            if (!ModelState.IsValid)
                return View(category);

            try
            {
                await _categoryService.CreateCategoryAsync(category);
                return RedirectToAction("Details", "Budget", new { id = category.BudgetId });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(category);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryDetailsAsync(id);
                return View(category);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _categoryService.DeleteCategoryAsync(id);

            if (!success)
                return NotFound("Category not found.");

            return RedirectToAction("Index", "Budget");
        }
    }
}
