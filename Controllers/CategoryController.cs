using BudgetTracker.Interfaces;
using BudgetTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Controllers
{
// ***********
// CategoryController Class
// ***********

// The CategoryController class manages category-related operations and inherits from the Controller base class.
// The Controller base class provides built-in methods like View(), RedirectToAction(), and Json() to handle various HTTP responses.

public class CategoryController : Controller
{
    // Private field to hold the service reference
    // Creates an object of ICategoryService that allows us to use the implementation methods of the CategoryService class.
    // readonly ensures that the _categoryService field can only be assigned once, preventing accidental modifications later in the controller.
    private readonly ICategoryService _categoryService;

    // ***********
    // Constructor
    // ***********

    // Initializes the CategoryController with the required ICategoryService via dependency injection.
    // This approach promotes loose coupling and allows the controller to depend on an abstraction rather than a concrete implementation.

    public CategoryController(ICategoryService categoryService)
    {
        // Assign the ICategoryService instance provided by Dependency Injection to the private field _categoryService.
        // This enables the controller to use the methods defined in the ICategoryService interface 
        // (implemented by the CategoryService class) throughout this class.
        _categoryService = categoryService; // Category management
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
