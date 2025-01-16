using BudgetTracker.Models;

namespace BudgetTracker.Interfaces
{
    public interface ITransactionService
    {
        // Retrieves a collection of transactions associated with a specific category.
        // categoryId: The unique identifier of the category for which transactions are fetched.
        // Returns an enumerable collection of Transaction objects for the specified category.
        Task<IEnumerable<Transaction>> GetTransactionsByCategoryAsync(int categoryId);

        // Creates a new transaction in the system.
        // transaction: The Transaction object containing details of the transaction to be created.
        // Returns the newly created Transaction object.
        Task<Transaction> CreateTransactionAsync(Transaction transaction);

        // Deletes a specific transaction by its unique identifier.
        // transactionId: The unique identifier of the transaction to delete.
        Task DeleteTransactionAsync(int transactionId);

        // Retrieves a collection of recent transactions in the system.
        // Returns an enumerable collection of Transaction objects representing recent transactions.
        Task<IEnumerable<Transaction>> GetRecentTransactionsAsync();

        // Retrieves a specific transaction by its unique identifier.
        // id: The unique identifier of the transaction to retrieve.
        // Returns the Transaction object matching the given ID.
        Task<Transaction> GetTransactionByIdAsync(int id);

        // Updates an existing transaction in the system.
        // transaction: The updated Transaction object with new values.
        // Ensures the transaction in the database is updated with the provided values.
        Task UpdateTransactionAsync(Transaction transaction);
    }
}
