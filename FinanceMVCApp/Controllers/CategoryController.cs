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





    // ***********
    // GET Method for Creating a Category
    // ***********

    // Handles the GET request to display the form for creating a new category.
    // Pre-fills the form with the BudgetId to associate the category with a specific budget.
    // Parameters:
    // - `budgetId`: The ID of the budget the new category will be associated with.
    // Returns:
    // - The Create view pre-populated with the BudgetId.
    [HttpGet]
    public IActionResult Create(int budgetId)
    {
        // Initialize a new Category object and pre-fill the BudgetId.
        var category = new Category
        {
            BudgetId = budgetId // Pre-fill the BudgetId to associate the category with the correct budget.
        };

        // Pass the initialized category model to the Create view.
        return View(category);
    }


    // ***********
    // POST Method for Creating a Category
    // ***********

    // Handles the POST request to create a new category.
    // Validates the input model and creates a new category in the database.
    // Parameters:
    // - `category`: The Category object containing the form data submitted by the user.
    // Returns:
    // - Redirects to the Details page of the associated budget if successful.
    // - Returns the Create view with validation messages if the model state is invalid or an error occurs.
[HttpPost]
public async Task<IActionResult> Create(Category category)
{
    if (!ModelState.IsValid)
    {
        return View(category);
    }

    try
    {
        // Fetch the budget details to get the remaining budget amount
        var budget = await _categoryService.GetBudgetByIdAsync(category.BudgetId);
        if (budget == null)
        {
            ModelState.AddModelError("", "Budget not found.");
            return View(category);
        }

        // Check if the allocated amount exceeds the remaining budget
        if (category.AllocatedAmount > budget.RemainingAmount)
        {
            ModelState.AddModelError("AllocatedAmount", "The allocated amount exceeds the remaining budget.");
            return View(category);
        }

        // Create the new category
        await _categoryService.CreateCategoryAsync(category);
        TempData["SuccessMessage"] = "Successfully created your category.";
        return RedirectToAction("Details", "Budget", new { id = category.BudgetId });
    }
    catch (InvalidOperationException ex)
    {
        ModelState.AddModelError("", ex.Message);
        return View(category);
    }
}










    // ***********
    // GET Method for Viewing Category Details
    // ***********

    // Handles the GET request to display detailed information about a specific category.
    // Parameters:
    // - `id`: The ID of the category to fetch details for.
    // Returns:
    // - The Details view with the category data if found.
    // - A NotFound result if the category does not exist.
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            // Call the service to fetch the details of the specified category asynchronously.
            var category = await _categoryService.GetCategoryDetailsAsync(id);

            // Pass the retrieved category data to the Details view.
            return View(category);
        }
        catch (InvalidOperationException ex)
        {
            // Return a NotFound result with a specific error message if an exception occurs.
            return NotFound(ex.Message);
        }
    }




    // ***********
    // POST Method for Deleting a Category
    // ***********

    // Handles the deletion of a category by its ID.
    // Parameters:
    // - `id`: The ID of the category to delete.
    // Returns:
    // - A NotFound response if the category doesn't exist or the deletion fails.
    // - Redirects to the Budget Details page upon successful deletion with a confirmation message.
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        // Retrieve the category to get its associated BudgetId
        var category = await _categoryService.GetCategoryDetailsAsync(id);
        if (category == null)
        {
            return NotFound("Category not found.");
        }

        // Attempt to delete the category using the service
        var success = await _categoryService.DeleteCategoryAsync(id);

        if (!success)
        {
            return NotFound("Failed to delete the category.");
        }

        // Set a success message to display in the redirected view
        TempData["SuccessMessage"] = "Category deleted successfully.";

        // Redirect to the Budget Details page of the associated budget
        return RedirectToAction("Details", "Budget", new { id = category.BudgetId });
    }






    // ***********
    // GET Method for Editing a Category
    // ***********

    // Handles the GET request to display the form for editing a category.
    // Parameters:
    // - `id`: The ID of the category to edit.
    // Returns:
    // - The Edit view with the category data if found.
    // - A NotFound result if the category doesn't exist.
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        // Fetch the category details using the service.
        var category = await _categoryService.GetCategoryDetailsAsync(id);

        // Check if the category exists.
        if (category == null)
        {
            // Return a NotFound result with an error message if the category is not found.
            return NotFound("Category not found.");
        }

        // Pass the category data to the Edit view for display and editing.
        return View(category);
    }




    // ***********
    // POST Method for Editing a Category
    // ***********

    // Handles the POST request to save changes to an existing category.
    // Parameters:
    // - `category`: The updated Category object submitted from the form.
    // Returns:
    // - The Edit view with validation messages if the model state is invalid or an error occurs.
    // - Redirects to the Budget Details page upon successful update.
    [HttpPost]
    public async Task<IActionResult> Edit(Category category)
    {
        // Validate the submitted model.
        if (!ModelState.IsValid)
        {
            // Log validation errors for debugging purposes.
            Console.WriteLine("[DEBUG] ModelState is invalid.");
            foreach (var key in ModelState.Keys)
            {
                foreach (var error in ModelState[key].Errors)
                {
                    Console.WriteLine($"[DEBUG] ModelState Error - Key: {key}, Error: {error.ErrorMessage}");
                }
            }

            // Return the Edit view with the current category data to allow the user to fix input errors.
            return View(category);
        }

        try
        {
            // Update the category using the service.
            await _categoryService.UpdateCategoryAsync(category);
            Console.WriteLine("[DEBUG] Category updated successfully.");
        }
        catch (Exception ex)
        {
            // Log the exception for debugging purposes.
            Console.WriteLine($"[DEBUG] Exception occurred during update: {ex.Message}");

            // Add an error message to the model state to inform the user of the issue.
            ModelState.AddModelError(string.Empty, "An error occurred while updating the category. Please try again.");

            // Return the Edit view with the current category data and error message.
            return View(category);
        }

        // Redirect to the Budget Details page after a successful update.
        return RedirectToAction("Details", "Budget", new { id = category.BudgetId });
    }







    // End of Controller 

    }
}
