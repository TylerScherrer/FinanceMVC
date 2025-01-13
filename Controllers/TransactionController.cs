using BudgetTracker.Interfaces;
using BudgetTracker.Models;
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

        // ***********
        // Constructor
        // ***********

        /// Initializes a new instance of the `TransactionController` class.
        /// Uses Dependency Injection to provide the required service for transaction management.
        /// <param name="transactionService">An instance of a class implementing `ITransactionService`.</param>
        public TransactionController(ITransactionService transactionService)
        {
            // Assign the injected service instance to the private field.
            // This allows the controller to call methods defined in the `ITransactionService` interface.
            _transactionService = transactionService;
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
    /// - Handles form submission for creating a new transaction.
    /// - Validates the data and saves the transaction using the service.
    ///
    /// Parameters:
    /// - `transaction`: The transaction object submitted from the form.
    ///
    /// Returns:
    /// - Redirects to the `Category` details page upon success.
    /// - Reloads the `Create` view with validation errors upon failure.
    [HttpPost]
    public async Task<IActionResult> Create(Transaction transaction)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _transactionService.CreateTransactionAsync(transaction); // Save the transaction.
                return RedirectToAction("Details", "Category", new { id = transaction.CategoryId }); // Redirect to category details.
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message); // Add error to the model state for display in the view.
            }
        }

        return View(transaction); // Reload the view with the current model if validation fails.
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
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _transactionService.DeleteTransactionAsync(id); // Delete the transaction.
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message); // Log any errors during deletion.
        }

        return RedirectToAction("Details", "Category", new { id = id }); // Redirect to category details.
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
