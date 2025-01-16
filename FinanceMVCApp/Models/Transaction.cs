using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        [Required]
        public string Description { get; set; }

[Range(0.01, double.MaxValue, ErrorMessage = "Please enter a value greater than 0.")]
[RegularExpression(@"\d+(\.\d{1,2})?", ErrorMessage = "Please enter a valid amount with up to 2 decimal places.")]
public decimal Amount { get; set; }



        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        [Required]
        public int CategoryId { get; set; } // Foreign key to the Category

        public Category? Category { get; set; } // Navigation property
    }
}
