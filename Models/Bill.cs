using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.Models
{
    public class Bill
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } // e.g., "Electricity"

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        public bool IsPaid { get; set; } = false;

        [Required]
        public int BudgetId { get; set; } // Foreign key to Budget

        public Budget Budget { get; set; } // Navigation property
    }
}
