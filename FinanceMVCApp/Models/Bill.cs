using System;
using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.Models
{

    /// Represents a bill associated with a specific budget.
    public class Bill
    {
        /// Unique identifier for the bill.
        public int Id { get; set; }


        /// The name of the bill (e.g., "Electricity", "Internet").
        [Required] // Ensures the Name field is mandatory.
        public string Name { get; set; }


        /// The monetary amount of the bill.
        /// Must be a positive value.
        [Required] // Ensures the Amount field is mandatory.
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be a positive value.")] // Validates that the amount is non-negative.
        public decimal Amount { get; set; }


        /// The due date for the bill.
        [Required] 
        public DateTime DueDate { get; set; }


        /// Indicates whether the bill has been paid.
        [Required]
        public bool IsPaid { get; set; }


        /// The foreign key representing the associated budget.
        [Required] 
        public int BudgetId { get; set; }


        /// The associated budget for this bill.
        /// This relationship is optional, indicated by the nullable Budget property.
        public Budget? Budget { get; set; }
    }
}
