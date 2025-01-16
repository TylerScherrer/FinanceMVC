using BudgetTracker.Models;

namespace BudgetTracker.Interfaces
{
    public interface ICategoryService
    {
        // Creates a new category and associates it with a budget.
        // category: The Category object containing the details of the category to create.
        // Returns the created Category object after it is saved to the database.
        Task<Category> CreateCategoryAsync(Category category);

        // Retrieves detailed information about a specific category by its ID.
        // id: The unique identifier of the category to fetch.
        // Returns the Category object with the specified ID, or null if not found.
        Task<Category> GetCategoryDetailsAsync(int id);

        // Deletes a specific category by its ID.
        // id: The unique identifier of the category to delete.
        // Returns true if the category was successfully deleted, false otherwise.
        Task<bool> DeleteCategoryAsync(int id);

        // Updates the details of an existing category.
        // category: The Category object containing the updated details, including the ID.
        // Returns nothing. Saves the changes made to the category in the database.
        Task UpdateCategoryAsync(Category category);
    }
}
