using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetTracker.Models
{
    public class Budget
    {
        // Unique identifier for the budget.
        public int Id { get; set; }



        // Name of the budget (e.g., "Monthly Budget").
        public string Name { get; set; }



        // The total amount allocated to this budget.
        public decimal TotalAmount { get; set; }



        // The date the budget was created.
        public DateTime DateCreated { get; set; }



        // Represents a collection of categories that are associated with this budget.
        // Initialized as an empty list to ensure the property is never null, 
        // avoiding potential NullReferenceExceptions when accessing or modifying the collection.
        // This allows you to immediately add, remove, or iterate over categories linked to the budget.
        public ICollection<Category> Categories { get; set; } = new List<Category>();





        // Dynamically calculates the total amount currently allocated across all categories.
        // - Uses a lambda function to iterate through each category in the `Categories` collection.
        // - Retrieves the `AllocatedAmount` of each category and sums them using the `Sum` method.
        // - Includes null safety:
        //   - If `Categories` is null, the null-conditional operator (`?.`) prevents errors.
        //   - If the result is null, the null-coalescing operator (`?? 0`) ensures a default value of 0.
        // This property provides a live calculation of the total allocated amount whenever accessed.
        public decimal TotalAllocated => Categories?.Sum(c => c.AllocatedAmount) ?? 0;



        // The initial total amount allocated across all categories when the budget was created.
        // - Uses a lambda function to iterate through each category in the `Categories` collection.
        // - Retrieves the `InitialAllocatedAmount` of each category and sums them using the `Sum` method.
        // - Includes null safety:
        //   - If `Categories` is null, the null-conditional operator (`?.`) prevents errors.
        //   - If the result is null, the null-coalescing operator (`?? 0`) ensures a default value of 0.
        // This property reflects the allocation at the time of budget creation.
        public decimal TotalAllocatedInitial => Categories?.Sum(c => c.InitialAllocatedAmount) ?? 0;



        // The total amount spent across all transactions within all categories.
        // - Uses a nested lambda function to first iterate through the `Categories` collection.
        // - For each category, iterates through its `Transactions` collection.
        // - Retrieves the `Amount` of each transaction and sums them using the `Sum` method.
        // - Includes null safety:
        //   - If `Categories` or `Transactions` are null, the null-conditional operator (`?.`) prevents errors.
        //   - If the result is null, the null-coalescing operator (`?? 0`) ensures a default value of 0.
        // This property calculates the total expenditures made in the budget.
        public decimal TotalSpent => Categories?.Sum(c => c.Transactions.Sum(t => t.Amount)) ?? 0;



        // The remaining budget amount, calculated as the total budget amount (`TotalAmount`) 
        // minus the initial total allocated amount (`TotalAllocatedInitial`).
        // - This reflects the funds that are still available after accounting for the initial allocations.
        // - Null safety is handled by ensuring `TotalAllocatedInitial` is always at least 0.
        public decimal RemainingAmount => TotalAmount - TotalAllocatedInitial;




        // A list of recent transactions associated with this budget.
        // This property is marked as [NotMapped] because it is not stored in the database.
        [NotMapped]
        public List<Transaction>? RecentTransactions { get; set; }



        // Collection of bills associated with this budget.
        public ICollection<Bill> Bills { get; set; } = new List<Bill>();



        // Property used for handling concurrency checks in the database.
        [Timestamp]
        [ScaffoldColumn(false)] // Prevents this property from being scaffolded in views.
                                // [ScaffoldColumn(false)] ensures that properties like RowVersion stay behind the scenes and are not exposed in auto-generated user interfaces.
                                // This keeps the application cleaner and more secure.
        public byte[] RowVersion { get; set; }
    }
}
