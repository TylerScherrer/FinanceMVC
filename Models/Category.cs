using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.Models
{
public class Category
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } // e.g., "Entertainment"

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal AllocatedAmount { get; set; }

    public List<Transaction> Transactions { get; set; } = new List<Transaction>();



    [Required]
    public int BudgetId { get; set; } // Foreign key linking to Budget

        // Navigation property (optional, EF Core resolves this using BudgetId)
    public Budget? Budget { get; set; }
}

}
