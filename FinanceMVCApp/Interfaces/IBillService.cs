using BudgetTracker.Models;

namespace BudgetTracker.Interfaces
{
    public interface IBillService
    {
        /// <summary>
        /// Retrieves all bills associated with a specific budget.
        /// </summary>
        /// <param name="budgetId">The ID of the budget.</param>
        /// <returns>A list of bills.</returns>
        Task<List<Bill>> GetBillsAsync(int budgetId);

        /// <summary>
        /// Creates a new bill.
        /// </summary>
        /// <param name="bill">The bill to create.</param>
        /// <returns>The created bill.</returns>
        Task CreateBillAsync(Bill bill);



        /// <summary>
        /// Deletes a bill by its ID.
        /// </summary>
        /// <param name="id">The ID of the bill to delete.</param>
        /// <returns>A boolean indicating whether the bill was successfully deleted.</returns>
        Task<bool> DeleteBillAsync(int id);
        Task<bool> MarkAsPaidAsync(int id);
        Task UpdateBillAsync(Bill bill); // Ensure this exists too
        Task<Bill> GetBillByIdAsync(int id); // Add this method
        Task<List<Bill>> GetBillsForMonthAsync(int month, int year);

    }
}
