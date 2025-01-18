using BudgetTracker.Data;
using BudgetTracker.Interfaces;
using BudgetTracker.Models;
using Microsoft.EntityFrameworkCore;


namespace BudgetTracker.Services
{
    /// <summary>
    /// Provides services related to budget operations, including CRUD operations and business logic.
    /// </summary>
    public class BudgetService : IBudgetService
    {
        // A reference to the application's database context for interacting with the database.
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="BudgetService"/> class.
        /// </summary>
        /// <param name="context">The database context used to access budget data.</param>
        /// <remarks>
        /// This constructor uses dependency injection to provide an instance of the
        /// <see cref="ApplicationDbContext"/>. This ensures that the service can interact
        /// with the database without directly managing its lifecycle.
        /// </remarks>
        public BudgetService(ApplicationDbContext context)
        {
            _context = context; // Assign the provided database context to the private field.
        }




    //**************
    // GET ALL BUDGETS 
    //**************

    /// <summary>
    /// Retrieves a list of all budgets from the database, including associated categories and their transactions.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a list of 
    /// <see cref="Budget"/> objects, each including its related categories and transactions.
    /// </returns>
    /// <remarks>
    /// - This method uses Entity Framework's eager loading to fetch related entities (`Categories` and their `Transactions`).
    /// - Eager loading helps ensure that all necessary data is retrieved in a single query to reduce database round-trips.
    /// - The method returns all budgets stored in the database. If no budgets exist, the result will be an empty list.
    /// </remarks>
    public async Task<List<Budget>> GetAllBudgetsAsync()
    {
        return await _context.Budgets
            // Includes associated categories for each budget
            .Include(b => b.Categories)
            // Includes transactions within each category
            .ThenInclude(c => c.Transactions)
            // Executes the query asynchronously and returns the results as a list
            .ToListAsync();
    }



    //**************
    // GET BUDGET DETAILS
    //**************

    /// <summary>
    /// Retrieves detailed information about a specific budget, including its associated categories and transactions.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the budget to retrieve.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the <see cref="Budget"/> object 
    /// with its related categories and transactions included.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no budget with the specified ID exists in the database.
    /// </exception>
    /// <remarks>
    /// - This method uses Entity Framework Core to retrieve a budget and its related entities in a single query.
    /// - Eager loading is used to include the `Categories` navigation property and their nested `Transactions`.
    /// </remarks>
    public async Task<Budget> GetBudgetDetailsAsync(int id)
    {
        // Retrieve the budget with the specified ID, including related categories and transactions.
        var budget = await _context.Budgets
            .Include(b => b.Categories)              // Include associated categories for the budget.
            .ThenInclude(c => c.Transactions)        // Include transactions within each category.
            .FirstOrDefaultAsync(b => b.Id == id);  // Find the budget matching the given ID.

        // Check if the budget was not found and throw an exception if necessary.
        if (budget == null)
            throw new InvalidOperationException("Budget not found.");

        // Return the fully populated budget.
        return budget;
    }







    //**************
    // CREATE A BUDGET
    //**************

    /// <summary>
    /// Creates a new budget and adds it to the database.
    /// </summary>
    /// <param name="budget">
    /// The <see cref="Budget"/> object to be added. Must contain valid data.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the newly created <see cref="Budget"/> object.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the <see cref="Budget.TotalAmount"/> is negative or the <see cref="Budget.Name"/> is null or empty.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if a budget with the same name already exists in the database.
    /// </exception>
    /// <remarks>
    /// - The method validates the budget's total amount and name before proceeding.
    /// - Ensures the RowVersion property is initialized for concurrency control.
    /// - Performs a duplicate name check to enforce uniqueness.
    /// - Adds the new budget to the database and commits the changes.
    /// </remarks>
    public async Task<Budget> CreateBudgetAsync(Budget budget)
    {
        // Validate that the total amount is non-negative.
        if (budget.TotalAmount < 0)
        {
            throw new ArgumentException("TotalAmount cannot be negative.", nameof(budget.TotalAmount));
        }

        // Validate that the name is not null, empty, or whitespace.
        if (string.IsNullOrWhiteSpace(budget.Name))
        {
            throw new ArgumentException("Name cannot be null or empty.", nameof(budget.Name));
        }

        // Initialize the RowVersion property if it has not been set.
        // RowVersion is used for concurrency control in Entity Framework.
        if (budget.RowVersion == null)
        {
            budget.RowVersion = new byte[8]; // Default size for a RowVersion field in SQL Server.
        }

        // Check if a budget with the same name already exists.
        // AsNoTracking() is used to avoid EF Core tracking this entity since it's only needed for validation.
        var existingBudget = await _context.Budgets
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Name == budget.Name);

        if (existingBudget != null)
        {
            // Throw an exception if a budget with the same name is found.
            throw new InvalidOperationException("A budget with the same name already exists.");
        }

        // Add the new budget to the database context.
        // This does not immediately save the changes to the database but marks the entity for addition.
        _context.Budgets.Add(budget);

        // Save the changes to the database asynchronously.
        // This commits the new budget and ensures all validations and constraints at the database level are enforced.
        await _context.SaveChangesAsync();

        // Return the newly created budget object.
        return budget;
    }






    //**************
    // UPDATE A BUDGET
    //**************

    /// <summary>
    /// Updates an existing budget in the database.
    /// </summary>
    /// <param name="budget">
    /// The <see cref="Budget"/> object containing updated information.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the updated <see cref="Budget"/> object.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the <see cref="Budget.Name"/> is null or empty.
    /// </exception>
    /// <exception cref="DbUpdateConcurrencyException">
    /// Thrown if a concurrency conflict occurs, such as when another user or process has modified or deleted the budget.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the budget was deleted during the update process.
    /// </exception>
    /// <remarks>
    /// - Ensures input validation for critical fields like <see cref="Budget.Name"/>.
    /// - Detaches any existing tracked entity to prevent state conflicts.
    /// - Uses optimistic concurrency by validating the <see cref="Budget.RowVersion"/> to detect conflicts.
    /// - Provides detailed error handling for concurrency issues.
    /// </remarks>
    public async Task<Budget> UpdateBudgetAsync(Budget budget)
    {
        try
        {
            // Validate the budget name to ensure it is not null or empty.
            if (string.IsNullOrWhiteSpace(budget.Name))
            {
                throw new ArgumentException("Name cannot be null or empty.", nameof(budget.Name));
            }

            // Check if the entity is already being tracked by the ChangeTracker.
            var trackedEntity = _context.ChangeTracker.Entries<Budget>()
                .FirstOrDefault(e => e.Entity.Id == budget.Id);

            if (trackedEntity != null)
            {
                // Detach the tracked entity to prevent state conflicts.
                _context.Entry(trackedEntity.Entity).State = EntityState.Detached;
            }

            // Attach the provided budget entity to the context and set its original RowVersion.
            // This ensures that concurrency is handled correctly during the update.
            _context.Entry(budget).Property(b => b.RowVersion).OriginalValue = budget.RowVersion;

            // Mark the entity as modified to signal EF Core that it should be updated.
            _context.Budgets.Update(budget);

            // Save the changes asynchronously to the database.
            await _context.SaveChangesAsync();

            // Return the updated budget object.
            return budget;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Handle concurrency conflicts by iterating through the entries in the exception.
            foreach (var entry in ex.Entries)
            {
                if (entry.Entity is Budget)
                {
                    // Attempt to retrieve the current database values for the budget.
                    var databaseValues = await entry.GetDatabaseValuesAsync();

                    if (databaseValues == null)
                    {
                        // If no database values exist, the budget was deleted.
                        throw new InvalidOperationException("Budget was deleted by another user.");
                    }

                    // Convert the database values to a Budget object for conflict resolution.
                    var dbValues = (Budget)databaseValues.ToObject();

                    // Optionally, return or log the database values for resolving the conflict.
                    throw new DbUpdateConcurrencyException("The budget was updated by another user.", ex);
                }
            }

            // Re-throw the exception if no specific handling is required for other entities.
            throw;
        }
    }











    //**************
    // DELETE A BUDGET
    //**************

    /// <summary>
    /// Deletes a budget from the database by its ID.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the budget to be deleted.
    /// </param>
    /// <returns>
    /// A <see cref="bool"/> indicating whether the deletion was successful:
    /// - <c>true</c> if the budget was found and deleted.
    /// - <c>false</c> if the budget does not exist.
    /// </returns>
    /// <remarks>
    /// This method retrieves the budget using its ID. If found, it removes the budget
    /// from the database and commits the changes. If the budget does not exist, it returns <c>false</c>.
    /// </remarks>
    public async Task<bool> DeleteBudgetAsync(int id)
    {
        // Attempt to find the budget entity in the database using the provided ID.
        var budget = await _context.Budgets.FindAsync(id);

        // If no budget is found with the given ID, return false to indicate failure.
        if (budget == null) 
            return false;

        // Remove the budget entity from the DbContext, marking it for deletion.
        _context.Budgets.Remove(budget);

        // Commit the changes asynchronously to ensure the deletion is reflected in the database.
        await _context.SaveChangesAsync();

        // Return true to indicate the budget was successfully deleted.
        return true;
    }



























    // End of Budget Services        
    }
}
