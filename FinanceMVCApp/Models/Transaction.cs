using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.Models
{
    public class Transaction
    {
        // Unique identifier for the transaction.
        public int Id { get; set; }

        // Description of the transaction, such as "Groceries" or "Gas Bill".
        // This field is required to ensure every transaction has a meaningful label.
        [Required]
        public string Description { get; set; }

        // The monetary value of the transaction.
        // - Must be greater than 0, as negative or zero values are not allowed.
        // - Includes validation for up to 2 decimal places to ensure precision in currency values.
        [Range(0.01, double.MaxValue, ErrorMessage = "Please enter a value greater than 0.")]
        [RegularExpression(@"\d+(\.\d{1,2})?", ErrorMessage = "Please enter a valid amount with up to 2 decimal places.")]
        public decimal Amount { get; set; }

        // The date when the transaction occurred.
        // - Defaults to the current date and time if not explicitly provided.
        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        // The ID of the category to which this transaction belongs.
        // Serves as a foreign key linking to the `Category` entity.
        [Required]
        public int CategoryId { get; set; }

        // Navigation property to the `Category` entity.
        // Allows access to detailed information about the category associated with this transaction.
        public Category? Category { get; set; }
    }
}
