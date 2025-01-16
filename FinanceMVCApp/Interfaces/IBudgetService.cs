using BudgetTracker.Models;

namespace BudgetTracker.Interfaces
{
    public interface IBudgetService
    {
        // Retrieves all budgets in the system.
        // Returns a list of Budget objects.
        Task<List<Budget>> GetAllBudgetsAsync();

        // Retrieves detailed information about a specific budget by its ID.
        // id: The unique identifier of the budget.
        // Returns the Budget object with the specified ID, or null if not found.
        Task<Budget> GetBudgetDetailsAsync(int id);

        // Creates a new budget and saves it to the database.
        // budget: The Budget object containing the details of the new budget.
        // Returns the created Budget object with any generated properties, like IDs.
        Task<Budget> CreateBudgetAsync(Budget budget);

        // Updates an existing budget with new details.
        // budget: The Budget object containing the updated details, including the ID.
        // Returns the updated Budget object after saving changes.
        Task<Budget> UpdateBudgetAsync(Budget budget);

        // Deletes a specific budget by its ID.
        // id: The unique identifier of the budget to delete.
        // Returns true if the deletion was successful, false otherwise.
        Task<bool> DeleteBudgetAsync(int id);
    }
}
