using BudgetTracker.Models;

namespace BudgetTracker.Interfaces
{
    public interface IBudgetService
    {
        Task<List<Budget>> GetAllBudgetsAsync();
        Task<Budget> GetBudgetDetailsAsync(int id);
        Task<Budget> CreateBudgetAsync(Budget budget);
        Task<Budget> UpdateBudgetAsync(Budget budget);
        Task<bool> DeleteBudgetAsync(int id);
    }
}
