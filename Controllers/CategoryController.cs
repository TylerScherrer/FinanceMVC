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

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryService.GetCategoryDetailsAsync(id);

            if (category == null)
            {
                return NotFound("Category not found.");
            }

            return View(category);
        }

[HttpPost]
public async Task<IActionResult> Edit(Category category)
{
    Console.WriteLine("[DEBUG] Received Form Data:");
    foreach (var key in Request.Form.Keys)
    {
        Console.WriteLine($"[DEBUG] Key: {key}, Value: {Request.Form[key]}");
    }

    Console.WriteLine($"[DEBUG] Bound Category Name: {category.Name}");
    Console.WriteLine($"[DEBUG] Bound Initial Allocated Amount: {category.InitialAllocatedAmount}");
    Console.WriteLine($"[DEBUG] Bound Allocated Amount: {category.AllocatedAmount}");

    if (!ModelState.IsValid)
    {
        Console.WriteLine("[DEBUG] ModelState is invalid.");
        foreach (var key in ModelState.Keys)
        {
            foreach (var error in ModelState[key].Errors)
            {
                Console.WriteLine($"[DEBUG] ModelState Error - Key: {key}, Error: {error.ErrorMessage}");
            }
        }
        return View(category); // Return the view with the current category to fix input errors
    }

    try
    {
        // Update the category using the service
        await _categoryService.UpdateCategoryAsync(category);
        Console.WriteLine("[DEBUG] Category updated successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DEBUG] Exception occurred during update: {ex.Message}");
        ModelState.AddModelError(string.Empty, "An error occurred while updating the category. Please try again.");
        return View(category); // Return the view with an error message
    }

    // Redirect to the budget details after successful update
    return RedirectToAction("Details", "Budget", new { id = category.BudgetId });
}








    }
}
