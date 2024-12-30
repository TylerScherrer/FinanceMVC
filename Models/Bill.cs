using System;
using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.Models
{
    public class Bill
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be a positive value.")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public bool IsPaid { get; set; }

        [Required]
        public int BudgetId { get; set; } // Foreign key

        public Budget? Budget { get; set; } // Marked as optional
    }
}
