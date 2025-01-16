using BudgetTracker.Models;

namespace BudgetTracker.Interfaces
{
    public interface ICategoryService
    {
        Task<Category> CreateCategoryAsync(Category category);
        Task<Category> GetCategoryDetailsAsync(int id);
        Task<bool> DeleteCategoryAsync(int id);

        Task UpdateCategoryAsync(Category category);
    }
}
