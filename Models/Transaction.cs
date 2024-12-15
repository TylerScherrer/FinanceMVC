using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        [Required]
        public int CategoryId { get; set; } // Foreign key to the Category

        public Category? Category { get; set; } // Navigation property
    }
}
