using BudgetTracker.Models;

namespace BudgetTracker.Interfaces
{
    public interface ITransactionService
    {
        Task<IEnumerable<Transaction>> GetTransactionsByCategoryAsync(int categoryId);
        Task<Transaction> CreateTransactionAsync(Transaction transaction);
        Task DeleteTransactionAsync(int transactionId);
        Task<IEnumerable<Transaction>> GetRecentTransactionsAsync();
    }
}
