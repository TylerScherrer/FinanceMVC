using BudgetTracker.Data;
using BudgetTracker.Interfaces;
using BudgetTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ApplicationDbContext _context;

        public TransactionService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Transaction>> GetTransactionsByCategoryAsync(int categoryId)
        {
            return await _context.Transactions
                .Where(t => t.CategoryId == categoryId)
                .ToListAsync();
        }
        public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
        {
            var category = await _context.Categories
                .Include(c => c.Transactions) // Include transactions
                .FirstOrDefaultAsync(c => c.Id == transaction.CategoryId);

            if (category == null)
            {
                throw new InvalidOperationException("Category not found.");
            }

            // Ensure the transaction amount does not exceed the allocated amount
            if (category.AllocatedAmount < transaction.Amount)
            {
                throw new InvalidOperationException("Transaction amount exceeds the allocated category amount.");
            }

            // Add the transaction
            category.Transactions.Add(transaction);

            // Update the allocated amount to reflect the transaction
            category.AllocatedAmount -= transaction.Amount;

            // Save changes
            await _context.SaveChangesAsync();

            return transaction;
        }


        public async Task DeleteTransactionAsync(int transactionId)
        {
            var transaction = await _context.Transactions.FindAsync(transactionId);

            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Transaction>> GetRecentTransactionsAsync(int count = 5)
        {
            return await _context.Transactions
                .OrderByDescending(t => t.Date)
                .Take(count)
                .ToListAsync();
        }
        public async Task<IEnumerable<Transaction>> GetRecentTransactionsAsync()
        {
            return await _context.Transactions
                .OrderByDescending(t => t.Date) // Order transactions by date, most recent first
                .Take(5) // Limit to the 5 most recent transactions
                .ToListAsync();
        }
    }
}
