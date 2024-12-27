using BudgetTracker.Data;
using BudgetTracker.Interfaces;
using BudgetTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly ApplicationDbContext _context;

        public BudgetService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Budget>> GetAllBudgetsAsync()
        {
            return await _context.Budgets
                .Include(b => b.Categories) // Include related categories
                .ToListAsync();
        }

        public async Task<Budget> GetBudgetDetailsAsync(int id)
        {
            var budget = await _context.Budgets
                .Include(b => b.Categories)
                    .ThenInclude(c => c.Transactions) // Include Transactions
                .FirstOrDefaultAsync(b => b.Id == id);

            if (budget == null)
                throw new InvalidOperationException("Budget not found.");

            return budget;
        }

        public async Task<Budget> CreateBudgetAsync(Budget budget)
        {
            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();
            return budget;
        }

        public async Task<Budget> UpdateBudgetAsync(Budget budget)
        {
            var existingBudget = await _context.Budgets.FindAsync(budget.Id);
            if (existingBudget == null)
                throw new InvalidOperationException("Budget not found.");

            existingBudget.Name = budget.Name;
            existingBudget.TotalAmount = budget.TotalAmount;
            // Update other fields as needed...

            _context.Budgets.Update(existingBudget);
            await _context.SaveChangesAsync();
            return existingBudget;
        }

        public async Task<bool> DeleteBudgetAsync(int id)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null) return false;

            _context.Budgets.Remove(budget);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
