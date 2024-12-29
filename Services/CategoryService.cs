using BudgetTracker.Data;
using BudgetTracker.Interfaces;
using BudgetTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            var budget = await _context.Budgets
                .Include(b => b.Categories)
                .FirstOrDefaultAsync(b => b.Id == category.BudgetId);

            if (budget == null)
                throw new InvalidOperationException("Budget not found.");

            if (budget.RemainingAmount < category.AllocatedAmount)
                throw new InvalidOperationException("Allocated amount exceeds the remaining budget.");

            category.InitialAllocatedAmount = category.AllocatedAmount;
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return category;
        }


        public async Task<Category> GetCategoryDetailsAsync(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Budget)
                .Include(c => c.Transactions)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                throw new InvalidOperationException("Category not found.");

            return category;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;

            var budget = await _context.Budgets.FindAsync(category.BudgetId);
            if (budget != null)
            {
                // You can adjust any budget-related calculations here if needed
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
