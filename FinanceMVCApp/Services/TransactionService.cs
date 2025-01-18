using BudgetTracker.Data;
using BudgetTracker.Interfaces;
using BudgetTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Services
{
    /// <summary>
    /// Provides methods to manage and interact with transactions within the budget tracker application.
    /// Handles creating, retrieving, updating, and deleting transactions, ensuring proper database interactions.
    /// </summary>
    public class TransactionService : ITransactionService
    {
        // The database context used for interacting with the application's data.
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionService"/> class.
        /// </summary>
        /// <param name="context">The application's database context, injected via dependency injection.</param>
        /// <remarks>
        /// This constructor establishes a connection between the service and the database layer,
        /// enabling all subsequent database operations to be executed using the provided context.
        /// </remarks>
        public TransactionService(ApplicationDbContext context)
        {
            // Assign the injected ApplicationDbContext to the private field _context.
            // This context is used for performing all database operations in the service.
            _context = context ?? throw new ArgumentNullException(nameof(context), "Database context cannot be null.");
        }
    


    //**************
    // GET TRANSACTION BY CATEGORY 
    //**************

    /// <summary>
    /// Retrieves all transactions associated with a specific category.
    /// </summary>
    /// <param name="categoryId">The unique identifier of the category whose transactions are to be retrieved.</param>
    /// <returns>
    /// An asynchronous operation that resolves to an <see cref="IEnumerable{Transaction}"/> containing all transactions 
    /// linked to the specified category.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the provided <paramref name="categoryId"/> is invalid (e.g., zero or negative).
    /// </exception>
    /// <remarks>
    /// This method queries the database for all transactions where the category ID matches the specified value. 
    /// It ensures that only transactions related to the given category are retrieved.
    /// </remarks>
    public async Task<IEnumerable<Transaction>> GetTransactionsByCategoryAsync(int categoryId)
    {
        // Validate the input parameter
        if (categoryId <= 0)
        {
            throw new ArgumentException("Category ID must be greater than zero.", nameof(categoryId));
        }

        // Query the database for transactions linked to the specified category ID
        // and asynchronously retrieve them as a list.
        return await _context.Transactions
            .Where(t => t.CategoryId == categoryId)
            .ToListAsync();
    }



    //**************
    // CREATE A TRANSACTION
    //**************

    /// <summary>
    /// Creates a new transaction and associates it with a specified category.
    /// </summary>
    /// <param name="transaction">The <see cref="Transaction"/> object containing transaction details such as amount, category ID, etc.</param>
    /// <returns>
    /// The newly created <see cref="Transaction"/> object with updated details after saving to the database.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if:
    /// - The category specified by <paramref name="transaction.CategoryId"/> does not exist.
    /// - The transaction amount exceeds the allocated amount for the category.
    /// </exception>
    /// <remarks>
    /// This method performs the following steps:
    /// 1. Validates that the category exists and retrieves it from the database.
    /// 2. Checks that the transaction amount does not exceed the remaining allocated amount for the category.
    /// 3. Associates the transaction with the category and updates the remaining allocated amount.
    /// 4. Saves the changes to the database.
    /// </remarks>
    public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
    {
        // Step 1: Retrieve the category from the database, including its associated transactions
        var category = await _context.Categories
            .Include(c => c.Transactions) // Ensures related transactions are loaded
            .FirstOrDefaultAsync(c => c.Id == transaction.CategoryId);

        // Step 2: Check if the category exists
        if (category == null)
        {
            throw new InvalidOperationException("Category not found."); // Throw an error if the category is missing
        }

        // Step 3: Ensure the transaction amount does not exceed the allocated amount for the category
        if (category.AllocatedAmount < transaction.Amount)
        {
            throw new InvalidOperationException("Transaction amount exceeds the allocated category amount.");
        }

        // Step 4: Add the transaction to the category's transaction collection
        category.Transactions.Add(transaction);

        // Step 5: Deduct the transaction amount from the category's allocated amount
        category.AllocatedAmount -= transaction.Amount;

        // Step 6: Save the changes to the database
        await _context.SaveChangesAsync();

        // Step 7: Return the created transaction
        return transaction;
    }


    //**************
    // DELETE A TRANSACTION
    //**************

    /// <summary>
    /// Deletes a transaction from the database based on its ID.
    /// </summary>
    /// <param name="transactionId">The unique identifier of the transaction to be deleted.</param>
    /// <returns>
    /// A completed task when the operation is finished.
    /// </returns>
    /// <remarks>
    /// This method performs the following steps:
    /// 1. Retrieves the transaction from the database by its ID.
    /// 2. Checks if the transaction exists.
    /// 3. If found, removes the transaction from the database.
    /// 4. Saves the changes asynchronously to persist the removal.
    /// If the transaction does not exist, the method performs no action.
    /// </remarks>
    public async Task DeleteTransactionAsync(int transactionId)
    {
        // Step 1: Retrieve the transaction by its ID from the database
        var transaction = await _context.Transactions.FindAsync(transactionId);

        // Step 2: Check if the transaction exists
        if (transaction != null) // Proceed only if a matching transaction is found
        {
            // Step 3: Remove the transaction from the database context
            _context.Transactions.Remove(transaction);

            // Step 4: Save changes to persist the deletion
            await _context.SaveChangesAsync();
        }
    }








    //**************
    // GET RECENT TRANSACTIONS
    //**************

    /// <summary>
    /// Retrieves the 5 most recent transactions from the database.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation, containing a list of the 5 most recent transactions.
    /// </returns>
    /// <remarks>
    /// This method retrieves transactions sorted by date in descending order, returning up to 5 transactions.
    /// If there are fewer than 5 transactions in the database, all available transactions are returned.
    /// The query is executed asynchronously for efficient database interaction.
    /// </remarks>
    public async Task<IEnumerable<Transaction>> GetRecentTransactionsAsync()
    {
        // Query the database for transactions
        // Step 1: Sort transactions by date in descending order (most recent first)
        // Step 2: Limit the result to the 5 most recent transactions
        // Step 3: Execute the query asynchronously and return the results as a list
        return await _context.Transactions
            .OrderByDescending(t => t.Date) // Sort transactions by date, most recent first
            .Take(5)                        // Retrieve only the top 5 transactions
            .ToListAsync();                 // Convert query results to a list asynchronously
    }



    //**************
    // UPDATE A TRANSACTION
    //**************

    /// <summary>
    /// Updates an existing transaction with new details.
    /// </summary>
    /// <param name="updatedTransaction">The transaction object containing the updated details.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if:
    /// - The transaction is not found in the database.
    /// - No significant change (at least 0.01) is made to the transaction amount.
    /// </exception>
    /// <remarks>
    /// This method ensures that transactions are properly normalized and updated in the database.
    /// If the transaction amount is updated, the value is rounded to two decimal places for consistency.
    /// </remarks>
    public async Task UpdateTransactionAsync(Transaction updatedTransaction)
    {
        // Find the existing transaction in the database by its ID
        var existingTransaction = await _context.Transactions.FindAsync(updatedTransaction.Id);
        if (existingTransaction == null)
        {
            // Throw an exception if the transaction is not found
            throw new InvalidOperationException("Transaction not found.");
        }

        // Normalize the amount values to two decimal places
        updatedTransaction.Amount = Math.Round(updatedTransaction.Amount, 2);
        existingTransaction.Amount = Math.Round(existingTransaction.Amount, 2);

        // Check if the updated amount is identical to the existing amount
        if (updatedTransaction.Amount == existingTransaction.Amount)
        {
            // Throw an exception if no significant change is made to the amount
            throw new InvalidOperationException("Please make a change of at least 0.01 to the amount.");
        }

        // Update the fields of the existing transaction with the new values
        existingTransaction.Description = updatedTransaction.Description;
        existingTransaction.Amount = updatedTransaction.Amount;
        existingTransaction.Date = updatedTransaction.Date;

        // Save changes to the database
        await _context.SaveChangesAsync();
    }




    //**************
    // GET A TRANSACTION BY THE ID 
    //**************

    /// <summary>
    /// Retrieves a transaction by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the transaction to retrieve.</param>
    /// <returns>
    /// The transaction object if found, or null if no transaction with the given ID exists.
    /// </returns>
    /// <remarks>
    /// This method uses Entity Framework's `FindAsync` to perform a direct lookup of the transaction
    /// in the database by its primary key.
    /// </remarks>
    public async Task<Transaction> GetTransactionByIdAsync(int id)
    {
        // Perform a lookup in the database for the transaction with the specified ID.
        // `FindAsync` is optimized for primary key lookups and returns null if no match is found.
        return await _context.Transactions.FindAsync(id);
    }


















    // End of Transaction Services 
    }
}
