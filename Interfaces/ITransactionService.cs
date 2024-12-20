using BudgetTracker.Models;

namespace BudgetTracker.Interfaces
{
    public interface ITransactionService
    {
        Task<Transaction> CreateTransactionAsync(Transaction transaction);
        Task DeleteTransactionAsync(int transactionId);
    }
}
