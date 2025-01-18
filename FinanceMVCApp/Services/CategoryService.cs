using BudgetTracker.Data;
using BudgetTracker.Interfaces;
using BudgetTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Services
{
    /// <summary>
    /// Service class for managing category-related operations.
    /// Implements the <see cref="ICategoryService"/> interface to provide 
    /// functionality for interacting with category data in the database.
    /// </summary>
    public class CategoryService : ICategoryService
    {
        // Private field to store the application's database context
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryService"/> class.
        /// </summary>
        /// <param name="context">
        /// An instance of <see cref="ApplicationDbContext"/> that provides access
        /// to the database. Used to query, add, update, and delete category data.
        /// </param>
        /// <remarks>
        /// The <paramref name="context"/> is injected via dependency injection,
        /// ensuring that the service can interact with the database as configured
        /// in the application's DI container.
        /// </remarks>
        public CategoryService(ApplicationDbContext context)
        {
            // Assign the injected database context to the private field
            _context = context ?? throw new ArgumentNullException(nameof(context), "Database context cannot be null.");
        }


    //**************
    // CREATE A CATEGORY
    //**************

    /// <summary>
    /// Creates a new category and associates it with a specific budget.
    /// </summary>
    /// <param name="category">The category to be created, containing its budget reference and allocated amount.</param>
    /// <returns>The created category with its initial allocated amount and database-assigned ID.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the budget associated with the category does not exist, or if the allocated amount exceeds the remaining budget.
    /// </exception>
    /// <remarks>
    /// This method ensures that the allocated amount for the new category does not exceed the remaining budget.
    /// It also sets the `InitialAllocatedAmount` field to match the `AllocatedAmount` for tracking purposes.
    /// </remarks>
    public async Task<Category> CreateCategoryAsync(Category category)
    {
        // Retrieve the associated budget from the database, including its categories for validation.
        var budget = await _context.Budgets
            .Include(b => b.Categories) // Ensures we can access the budget's categories.
            .FirstOrDefaultAsync(b => b.Id == category.BudgetId); // Match the budget using the provided BudgetId.

        // If no matching budget is found, throw an exception to indicate an invalid operation.
        if (budget == null)
            throw new InvalidOperationException("Budget not found.");

        // Validate if the allocated amount for the category exceeds the remaining amount in the budget.
        if (budget.RemainingAmount < category.AllocatedAmount)
            throw new InvalidOperationException("Allocated amount exceeds the remaining budget.");

        // Initialize the `InitialAllocatedAmount` field to the same value as `AllocatedAmount` for consistency.
        category.InitialAllocatedAmount = category.AllocatedAmount;

        // Add the new category to the database context, marking it for insertion.
        _context.Categories.Add(category);

        // Save the changes to the database asynchronously.
        await _context.SaveChangesAsync();

        // Return the created category object, now updated with its database-generated ID and state.
        return category;
    }





    //**************
    // GET CATEGORY DETAILS 
    //**************

    /// <summary>
    /// Retrieves detailed information about a specific category, including its associated budget and transactions.
    /// </summary>
    /// <param name="id">The unique identifier of the category to retrieve.</param>
    /// <returns>
    /// A <see cref="Category"/> object containing detailed information, including its associated budget and transactions.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the category with the specified ID does not exist in the database.
    /// </exception>
    /// <remarks>
    /// This method performs the following steps:
    /// 1. Queries the database for the specified category, including its associated budget and transactions.
    /// 2. Throws an exception if the category is not found to ensure consistent error handling.
    /// 3. Returns the fully populated category object if found.
    /// </remarks>
    public async Task<Category> GetCategoryDetailsAsync(int id)
    {
        // Query the database for the category with the specified ID.
        // Includes related `Budget` and `Transactions` to provide a complete view of the category's details.
        var category = await _context.Categories
            .Include(c => c.Budget) // Include the associated budget details.
            .Include(c => c.Transactions) // Include the list of transactions linked to this category.
            .FirstOrDefaultAsync(c => c.Id == id); // Retrieve the first matching category or null if not found.

        // If no category is found, throw an exception indicating the category does not exist.
        if (category == null)
            throw new InvalidOperationException("Category not found.");

        // Return the category with its associated budget and transactions.
        return category;
    }





    //**************
    // DELETE A CATEGORY
    //**************

    /// <summary>
    /// Deletes a category by its unique identifier from the database.
    /// </summary>
    /// <param name="id">The unique identifier of the category to delete.</param>
    /// <returns>
    /// A boolean value:
    /// - <c>true</c> if the category was successfully deleted.
    /// - <c>false</c> if the category was not found in the database.
    /// </returns>
    /// <remarks>
    /// This method performs the following steps:
    /// 1. Attempts to find the category by its ID using <see cref="DbContext.FindAsync"/>.
    /// 2. Returns <c>false</c> if the category does not exist.
    /// 3. Retrieves the associated budget (if it exists) for potential adjustments or calculations.
    /// 4. Removes the category from the database and saves the changes.
    /// </remarks>
    public async Task<bool> DeleteCategoryAsync(int id)
    {
        // Retrieve the category from the database using its ID.
        var category = await _context.Categories.FindAsync(id);

        // If the category does not exist, return false to indicate failure.
        if (category == null) return false;

        // Retrieve the associated budget for the category, if available.
        var budget = await _context.Budgets.FindAsync(category.BudgetId);
        if (budget != null)
        {
            // Perform any necessary budget-related adjustments here.
            // For example, updating the budget's remaining amount or other fields.
        }

        // Mark the category for deletion in the database context.
        _context.Categories.Remove(category);

        // Commit the changes to the database to complete the deletion.
        await _context.SaveChangesAsync();

        // Return true to indicate successful deletion.
        return true;
    }




    //**************
    // UPDATE A CATEGORY
    //**************

    /// <summary>
    /// Updates an existing category's details in the database.
    /// </summary>
    /// <param name="category">The category object containing updated information.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the category with the specified ID does not exist in the database.
    /// </exception>
    /// <remarks>
    /// This method performs the following steps:
    /// 1. Retrieves the existing category by its ID.
    /// 2. Validates that the category exists in the database.
    /// 3. Updates the category's name and allocation details.
    /// 4. Saves the updated information to the database.
    /// </remarks>
    public async Task UpdateCategoryAsync(Category category)
    {
        // Attempt to retrieve the category from the database using its ID.
        var existingCategory = await _context.Categories.FindAsync(category.Id);

        // If the category does not exist, throw an exception to notify the caller.
        if (existingCategory == null)
        {
            throw new InvalidOperationException("Category not found.");
        }

        // Update the properties of the existing category with the new values.
        existingCategory.Name = category.Name; // Update the category name.
        existingCategory.InitialAllocatedAmount = category.InitialAllocatedAmount; // Update the initial allocation.
        existingCategory.AllocatedAmount = category.InitialAllocatedAmount; // Adjust the allocated amount to match the initial allocation.

        // Persist the changes to the database.
        await _context.SaveChangesAsync();
    }





    }
}
