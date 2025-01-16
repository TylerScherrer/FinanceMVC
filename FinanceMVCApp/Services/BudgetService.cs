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
        .Include(b => b.Categories)
        .Select(b => new Budget
        {
            Id = b.Id,
            Name = b.Name,
            TotalAmount = b.TotalAmount,
            DateCreated = b.DateCreated,
            RowVersion = b.RowVersion // Include RowVersion
        })
        .ToListAsync();
}

public async Task<Budget> GetBudgetDetailsAsync(int id)
{
    var budget = await _context.Budgets
        .Include(b => b.Categories)
            .ThenInclude(c => c.Transactions)
        .Select(b => new Budget
        {
            Id = b.Id,
            Name = b.Name,
            TotalAmount = b.TotalAmount,
            DateCreated = b.DateCreated,
            RowVersion = b.RowVersion // Include RowVersion
        })
        .FirstOrDefaultAsync(b => b.Id == id);

    if (budget == null)
        throw new InvalidOperationException("Budget not found.");

    return budget;
}

public async Task<Budget> CreateBudgetAsync(Budget budget)
{
    if (budget.TotalAmount < 0)
    {
        throw new ArgumentException("TotalAmount cannot be negative.", nameof(budget.TotalAmount));
    }

    // Ensure RowVersion is initialized
    if (budget.RowVersion == null)
    {
        budget.RowVersion = new byte[8]; // Initialize RowVersion with a default value
    }

    _context.Budgets.Add(budget);
    await _context.SaveChangesAsync();
    return budget;
}




public async Task<Budget> UpdateBudgetAsync(Budget budget)
{
    try
    {
        // Attach the entity to track changes
        _context.Entry(budget).Property(b => b.RowVersion).OriginalValue = budget.RowVersion;
        _context.Budgets.Update(budget);

        // Save changes
        await _context.SaveChangesAsync();
        return budget;
    }
    catch (DbUpdateConcurrencyException ex)
    {
        foreach (var entry in ex.Entries)
        {
            if (entry.Entity is Budget)
            {
                var databaseValues = await entry.GetDatabaseValuesAsync();
                if (databaseValues == null)
                {
                    throw new InvalidOperationException("Budget was deleted by another user.");
                }

                var dbValues = (Budget)databaseValues.ToObject();

                // Optionally return or log dbValues for resolving the conflict
                throw new DbUpdateConcurrencyException("The budget was updated by another user.", ex);
            }
        }

        throw;
    }
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
