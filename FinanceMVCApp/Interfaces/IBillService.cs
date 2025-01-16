using BudgetTracker.Models;

namespace BudgetTracker.Interfaces
{

    /// Defines the contract for the bill management service.
    /// This interface ensures that any implementation of the bill service 
    /// adheres to the specified methods for handling bill-related operations.
    public interface IBillService
    {

        /// Retrieves all bills associated with a specific budget.
        /// <param name="budgetId">The ID of the budget to retrieve bills for.</param>
        /// <returns>A list of bills associated with the specified budget.</returns>
        Task<List<Bill>> GetBillsAsync(int budgetId);



        /// Creates a new bill and associates it with the appropriate budget.
        /// <param name="bill">The bill object containing all required details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task CreateBillAsync(Bill bill);



        /// Deletes a bill by its unique ID.
        /// <param name="id">The unique ID of the bill to delete.</param>
        /// <returns>A boolean indicating whether the deletion was successful.</returns>
        Task<bool> DeleteBillAsync(int id);



        /// Marks a bill as paid.
        /// <param name="id">The unique ID of the bill to mark as paid.</param>
        /// <returns>A boolean indicating whether the operation was successful.</returns>
        Task<bool> MarkAsPaidAsync(int id);



        /// Updates the details of an existing bill.
        /// <param name="bill">The updated bill object.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateBillAsync(Bill bill);



        /// Retrieves a specific bill by its unique ID.
        /// <param name="id">The unique ID of the bill to retrieve.</param>
        /// <returns>The bill object with the specified ID, or null if not found.</returns>
        Task<Bill> GetBillByIdAsync(int id);

        

        /// Retrieves all bills for a specific month and year.
        /// <param name="month">The month (1-12) to filter bills.</param>
        /// <param name="year">The year to filter bills.</param>
        /// <returns>A list of bills due in the specified month and year.</returns>
        Task<List<Bill>> GetBillsForMonthAsync(int month, int year);
    }
}
