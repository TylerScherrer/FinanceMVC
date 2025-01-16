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
        .ThenInclude(c => c.Transactions) // Ensure nested inclusion if needed
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
    if (budget.TotalAmount < 0)
    {
        throw new ArgumentException("TotalAmount cannot be negative.", nameof(budget.TotalAmount));
    }

    if (string.IsNullOrWhiteSpace(budget.Name))
    {
        throw new ArgumentException("Name cannot be null or empty.", nameof(budget.Name));
    }

    // Ensure RowVersion is initialized
    if (budget.RowVersion == null)
    {
        budget.RowVersion = new byte[8]; // Initialize RowVersion with default value
    }

    // Check for duplicate names
    var existingBudget = await _context.Budgets
        .AsNoTracking()
        .FirstOrDefaultAsync(b => b.Name == budget.Name);

    if (existingBudget != null)
    {
        throw new InvalidOperationException("A budget with the same name already exists.");
    }

    _context.Budgets.Add(budget);
    await _context.SaveChangesAsync();
    return budget;
}






public async Task<Budget> UpdateBudgetAsync(Budget budget)
{
    try
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(budget.Name))
        {
            throw new ArgumentException("Name cannot be null or empty.", nameof(budget.Name));
        }

        // Check if the entity is already tracked
        var trackedEntity = _context.ChangeTracker.Entries<Budget>()
            .FirstOrDefault(e => e.Entity.Id == budget.Id);

        if (trackedEntity != null)
        {
            _context.Entry(trackedEntity.Entity).State = EntityState.Detached;
        }

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
