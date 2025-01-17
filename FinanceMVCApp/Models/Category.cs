using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.Models
{
    // Represents a category within a budget, such as "Entertainment" or "Groceries."
    // Each category can have a specific allocated amount and may contain multiple transactions.
    public class Category
    {
        // Unique identifier for the category.
        public int Id { get; set; }

        // Name of the category, such as "Entertainment" or "Groceries."
        [Required]
        public string Name { get; set; }

        // Amount of money allocated to this category within the budget. Must be greater than zero.
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal AllocatedAmount { get; set; }

        // Collection of transactions associated with this category.
        // Transactions record individual expenditures or additions linked to the category.
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        // Foreign key linking this category to its parent budget.
        // Ensures that each category is associated with a specific budget.
        [Required]
        public int BudgetId { get; set; }

        // Navigation property (optional) to represent the related budget entity.
        // This is resolved by EF Core using the BudgetId foreign key.
        public Budget? Budget { get; set; }

        // Stores the original amount allocated to this category when it was created.
        public decimal InitialAllocatedAmount { get; set; }
    }
}
