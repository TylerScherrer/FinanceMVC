using BudgetTracker.Interfaces;
using BudgetTracker.Models;
using BudgetTracker.Services;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Controllers
{

    
    /// ***********
    /// Transaction Controller
    /// ***********
    ///
    /// This controller handles requests and actions related to managing financial transactions.
    /// It provides endpoints for creating, updating, retrieving, and deleting transactions.
    ///
    /// Dependencies:
    /// - The controller relies on the `ITransactionService` interface to interact with the transaction data.
    ///
    /// Inherits:
    /// - Inherits from the `Controller` base class, which provides methods like `View()`, `RedirectToAction()`, and `Json()` for handling HTTP responses.
    public class TransactionController : Controller
    {
        // ***********
        // Dependencies
        // ***********

        /// Private, readonly field to hold the reference to the ITransactionService implementation.
        /// Used to interact with transaction-related data and operations.
        private readonly ITransactionService _transactionService;

        /// Private, readonly field to hold the reference to the ICategoryService implementation.
        /// Used to interact with category-related data and operations.
        private readonly ICategoryService _categoryService;

        // ***********
        // Constructor
        // ***********

        /// Initializes a new instance of the `TransactionController` class.
        /// Uses Dependency Injection to provide the required service for transaction management.
        /// <param name="transactionService">An instance of a class implementing `ITransactionService`.</param>
        /// <param name="categoryService">An instance of a class implementing `ICategoryService`.</param>
        public TransactionController(ICategoryService categoryService, ITransactionService transactionService)
        {
            // Assign the injected service instance to the private field.
            // This allows the controller to call methods defined in the `ITransactionService` interface.
            _transactionService = transactionService;

            // Assign the injected service instance to the private field.
            // This allows the controller to call methods defined in the `ICategoryService` interface.
            _categoryService = categoryService; // Injected service for category operations
        }





    /// ***********
    /// GET Method for Creating a Transaction
    /// ***********
    ///
    /// Purpose:
    /// - Renders the `Create` view with a pre-populated `Transaction` object.
    ///
    /// Parameters:
    /// - `categoryId`: The ID of the category to which the transaction belongs.
    ///
    /// Returns:
    /// - A view with the initialized `Transaction` model.
    [HttpGet]
    public IActionResult Create(int categoryId)
    {
        var transaction = new Transaction
        {
            CategoryId = categoryId // Pre-fill the CategoryId for the new transaction.
        };
        return View(transaction); // Render the Create view.
    }





    /// ***********
    /// POST Method for Creating a Transaction
    /// ***********
    ///
    /// Purpose:
    /// - Handles the form submission for creating a new transaction.
    /// - Validates the input data to ensure it meets business rules.
    /// - Prevents transactions that exceed the allocated amount for a specific category.
    ///
    /// Workflow:
    /// 1. Validate the model state for form submission errors.
    /// 2. Retrieve the category details associated with the transaction.
    /// 3. Check if the category exists.
    /// 4. Ensure the transaction amount does not exceed the remaining allocated amount for the category.
    /// 5. Save the transaction if all validations pass.
    /// 6. Redirect to the category details page upon success.
    /// 7. Display appropriate error messages and reload the form if any validation fails.
    ///
    /// Parameters:
    /// - `transaction`: A `Transaction` object containing the form data submitted by the user.
    ///
    /// Returns:
    /// - On success: Redirects to the `Details` page of the associated category.
    /// - On failure: Reloads the `Create` view and displays validation errors.
    ///
    /// Exceptions Handled:
    /// - If the category is not found, an error message is displayed to the user.
    /// - If the transaction amount exceeds the allocated amount, an error message is displayed.
    /// - If an exception occurs during the transaction saving process, an error is displayed.
    [HttpPost]
    public async Task<IActionResult> Create(Transaction transaction)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // Retrieve the category details for the transaction
                var category = await _categoryService.GetCategoryDetailsAsync(transaction.CategoryId);

                // Check if the category exists
                if (category == null)
                {
                    ModelState.AddModelError("", "Category not found.");
                    return View(transaction);
                }

                // Validate the transaction amount against the remaining category allocation
                if (transaction.Amount > category.AllocatedAmount)
                {
                    ModelState.AddModelError("", $"Transaction amount cannot exceed the remaining category amount of {category.AllocatedAmount:C}.");
                    return View(transaction);
                }

                // Save the transaction to the database
                await _transactionService.CreateTransactionAsync(transaction);

                // Set a success message for the user
                TempData["SuccessfulTransaction"] = "Your transaction was successfully created.";

                // Redirect to the category details page
                return RedirectToAction("Details", "Category", new { id = transaction.CategoryId });
            }
            catch (InvalidOperationException ex)
            {
                // Handle any business logic exceptions and display an error to the user
                ModelState.AddModelError("", ex.Message);
            }
        }

        // Reload the form with the current transaction data if validation fails
        return View(transaction);
    }






    /// ***********
    /// GET Method for Viewing Transactions by Category
    /// ***********
    ///
    /// Purpose:
    /// - Fetches all transactions associated with a specific category and displays them.
    ///
    /// Parameters:
    /// - `categoryId`: The ID of the category to fetch transactions for.
    ///
    /// Returns:
    /// - A view displaying the list of transactions for the category.
    /// - Returns `NotFound` if no transactions are found.
    [HttpGet]
    public async Task<IActionResult> Details(int categoryId)
    {
        var transactions = await _transactionService.GetTransactionsByCategoryAsync(categoryId);

        if (transactions == null)
        {
            return NotFound(); // Return 404 if no transactions found.
        }

        ViewBag.CategoryId = categoryId; // Pass the category ID to the view.
        return View(transactions); // Render the Details view with the transactions.
    }





    /// ***********
    /// POST Method for Deleting a Transaction
    /// ***********
    ///
    /// Purpose:
    /// - Handles the deletion of a transaction.
    ///
    /// Parameters:
    /// - `id`: The ID of the transaction to delete.
    ///
    /// Returns:
    /// - Redirects to the `Category` details page upon success or failure.
    [HttpPost]
    public async Task<IActionResult> Delete(int id, int categoryId)
    {
        try
        {
            // Delete the transaction using the service
            await _transactionService.DeleteTransactionAsync(id);
        }
        catch (Exception ex)
        {
            // Log any errors during deletion
            ModelState.AddModelError("", ex.Message);
        }

        // Redirect to the category details page with the category ID
        TempData["SuccessTransactionDelete"] = "Your transaction was succesfully deleted.";
        return RedirectToAction("Details", "Category", new { id = categoryId });
    }






    /// ***********
    /// GET Method for Editing a Transaction
    /// ***********
    ///
    /// Purpose:
    /// - Fetches a transaction by its ID and renders the `Edit` view.
    ///
    /// Parameters:
    /// - `id`: The ID of the transaction to edit.
    ///
    /// Returns:
    /// - A view pre-populated with the transaction details.
    /// - Returns `NotFound` if the transaction is not found.
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var transaction = await _transactionService.GetTransactionByIdAsync(id);
        if (transaction == null)
        {
            return NotFound(); // Return 404 if no transaction found.
        }
        return View(transaction); // Render the Edit view with the transaction data.
    }






    /// ***********
    /// POST Method for Editing a Transaction
    /// ***********
    ///
    /// Purpose:
    /// - Handles form submission for editing an existing transaction.
    ///
    /// Parameters:
    /// - `transaction`: The updated transaction object submitted from the form.
    ///
    /// Returns:
    /// - Redirects to the `Category` details page upon success.
    /// - Reloads the `Edit` view with validation errors upon failure.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Transaction transaction)
    {
        try
        {
            await _transactionService.UpdateTransactionAsync(transaction); // Update the transaction.
            return RedirectToAction("Details", "Category", new { id = transaction.CategoryId }); // Redirect to category details.
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message); // Add error message to the model state.
            return View(transaction); // Reload the Edit view with errors.
        }
    }




        




    // End of Controller 
    }
}
