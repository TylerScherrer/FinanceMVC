using BudgetTracker.Data;
using BudgetTracker.Interfaces;
using BudgetTracker.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetTracker.Services
{

    /// Service class responsible for handling business logic related to Bills.
    /// This class interacts with the underlying database through the ApplicationDbContext
    /// and provides methods to manage Bills, including creating, updating, deleting, 
    /// and retrieving Bills.
    public class BillService : IBillService
    {
        // Instance of the database context to interact with the database.
        private readonly ApplicationDbContext _context;


        /// Initializes a new instance of the <see cref="BillService"/> class.
        /// This constructor accepts an ApplicationDbContext instance,
        /// which is used to perform CRUD operations on the database.
        /// <param name="context">
        /// An instance of ApplicationDbContext injected via Dependency Injection.
        /// This ensures that the database context is managed by the DI container.
        public BillService(ApplicationDbContext context)
        {
            // Assign the injected ApplicationDbContext to the private field _context.
            // This ensures the service class has access to the database operations.
            _context = context;
        }


    //**************
    // GET BILL BY ID
    //**************

    /// Retrieves a bill by its unique identifier.
    /// <param name="id">The unique identifier of the bill to retrieve.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the 
    /// <see cref="Bill"/> object if found, or null if no bill exists with the specified ID.
    /// </returns>
    /// <remarks>
    /// This method uses Entity Framework's asynchronous capabilities to fetch the bill from
    /// the database. It queries the `Bills` DbSet and returns the first bill that matches the
    /// given ID. If no match is found, it returns null.
    public async Task<Bill> GetBillByIdAsync(int id)
    {
        // Use FirstOrDefaultAsync to retrieve the bill with the specified ID.
        // If no bill matches the condition (b.Id == id), the method returns null.
        return await _context.Bills.FirstOrDefaultAsync(b => b.Id == id);
    }




    //**************
    // GET ALL BILLS
    //**************

    /// Retrieves a list of bills associated with a specific budget.
    /// <param name="budgetId">The unique identifier of the budget whose bills are to be retrieved.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a list of 
    /// <see cref="Bill"/> objects associated with the specified budget.
    /// </returns>
    /// <remarks>
    /// This method queries the `Bills` DbSet in the database and filters the bills by the provided
    /// budget ID. It leverages Entity Framework's LINQ and asynchronous execution to efficiently
    /// retrieve the data.
    /// </remarks>
    public async Task<List<Bill>> GetBillsAsync(int budgetId)
    {
        // Fetch all bills where the BudgetId matches the provided budgetId
        // Convert the results to a list asynchronously.
        return await _context.Bills.Where(b => b.BudgetId == budgetId).ToListAsync();
    }




    //**************
    // CREATE A BILL
    //**************

    /// Adds a new bill to the database and persists the changes.
    /// <param name="bill">The <see cref="Bill"/> object to be added to the database.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// This method uses Entity Framework's `AddAsync` method to queue the bill for addition
    /// to the database. The changes are persisted to the database using `SaveChangesAsync`.
    /// Ensure that the provided `Bill` object has all required fields populated before calling
    /// this method to avoid validation errors.
    /// </remarks>
    public async Task CreateBillAsync(Bill bill)
    {
        // Add the bill to the DbSet asynchronously.
        await _context.Bills.AddAsync(bill);

        // Save the changes to the database.
        await _context.SaveChangesAsync();
    }




    //**************
    // DELETE A BILL
    //**************

    /// Deletes a bill from the database based on its unique identifier.
    /// <param name="id">The unique identifier of the bill to be deleted.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result is a boolean value:
    /// - <c>true</c> if the bill was successfully deleted.
    /// - <c>false</c> if no bill with the specified ID was found.
    /// </returns>
    /// <remarks>
    /// This method first retrieves the bill by its ID using <see cref="GetBillByIdAsync"/>. If the
    /// bill exists, it is removed from the database using Entity Framework's `Remove` method. The
    /// changes are then saved asynchronously using `SaveChangesAsync`.
    /// </remarks>
    public async Task<bool> DeleteBillAsync(int id)
    {
        // Attempt to retrieve the bill from the database using its unique ID.
        var bill = await GetBillByIdAsync(id);

        // If no bill is found, return false to indicate deletion was unsuccessful.
        if (bill == null)
            return false;

        // Remove the retrieved bill from the database context.
        _context.Bills.Remove(bill);

        // Persist the deletion changes to the database.
        await _context.SaveChangesAsync();

        // Return true to indicate the bill was successfully deleted.
        return true;
    }



    //**************
    // UPDATE A BILL
    //**************

    /// <summary>
    /// Updates an existing bill in the database with the provided details.
    /// </summary>
    /// <param name="bill">
    /// The <see cref="Bill"/> object containing the updated details. This object must have:
    /// - A valid `Id` that matches an existing bill in the database.
    /// - Any properties that are being updated correctly populated.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation to update the bill in the database.
    /// </returns>
    /// <remarks>
    /// - This method uses Entity Framework's `Update` method to mark the `bill` entity as modified.
    /// - It assumes that the `bill` object corresponds to an existing record in the database.
    /// - The changes are committed to the database by calling `SaveChangesAsync`.
    /// - The caller is responsible for ensuring that the `bill` entity is valid and contains an `Id`
    ///   corresponding to an existing record in the database.
    /// </remarks>
    /// <exception cref="DbUpdateConcurrencyException">
    /// Thrown if the bill being updated has been modified or deleted by another user or process
    /// since it was last retrieved.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the provided `bill` object is invalid or improperly configured.
    /// </exception>
    public async Task UpdateBillAsync(Bill bill)
    {
        // Marks the provided bill entity as modified within the database context.
        // This signals Entity Framework to prepare an update operation for this entity.
        _context.Bills.Update(bill);

        // Saves the changes asynchronously to the database.
        // This commits the updates for the bill entity to the underlying data store.
        await _context.SaveChangesAsync();
    }



    //**************
    // MARK A BILL PAID
    //**************

    /// <summary>
    /// Marks a specific bill as paid by updating its <see cref="IsPaid"/> property.
    /// </summary>
    /// <param name="billId">The unique identifier of the bill to mark as paid.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation.
    /// Returns <c>true</c> if the bill was successfully marked as paid; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method performs the following steps:
    /// 1. Retrieves the bill from the database using its ID.
    /// 2. If the bill is not found, returns <c>false</c>.
    /// 3. Updates the <see cref="IsPaid"/> property of the bill to <c>true</c>.
    /// 4. Saves the updated state to the database.
    /// </remarks>
    /// <exception cref="DbUpdateException">
    /// Thrown if an error occurs while saving changes to the database.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the database context is improperly configured or disposed during the operation.
    /// </exception>
    public async Task<bool> MarkAsPaidAsync(int billId)
    {
        // Retrieve the bill by its ID
        var bill = await _context.Bills.FindAsync(billId);

        if (bill == null)
        {
            // Return false if the bill is not found
            return false;
        }

        // Update the IsPaid property
        bill.IsPaid = true;

        // Save the changes to the database
        await _context.SaveChangesAsync();

        return true; // Return true indicating success
    }



    //**************
    // GET BILLS FOR A SPECIFIC MOTNTH
    //**************

    /// <summary>
    /// Retrieves a list of bills that are due in a specified month and year.
    /// </summary>
    /// <param name="month">The month for which bills should be retrieved (1 to 12).</param>
    /// <param name="year">The year for which bills should be retrieved.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation.
    /// The task result contains a <see cref="List{T}"/> of <see cref="Bill"/> objects due in the specified month and year.
    /// </returns>
    /// <remarks>
    /// This method queries the database for bills that match the provided month and year based on their <see cref="Bill.DueDate"/>.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if the provided <paramref name="month"/> is not between 1 and 12.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the database context is improperly configured or disposed during the operation.
    /// </exception>
    public async Task<List<Bill>> GetBillsForMonthAsync(int month, int year)
    {
        // Validate input parameters
        if (month < 1 || month > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");
        }

        // Query the database for bills matching the specified month and year
        return await _context.Bills
            .Where(b => b.DueDate.Month == month && b.DueDate.Year == year) // Filter by month and year
            .ToListAsync(); // Retrieve the results as a list
    }














    // End of Bill Services
    }
}
