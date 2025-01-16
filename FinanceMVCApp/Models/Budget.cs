using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetTracker.Models
{
public class Budget
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime DateCreated { get; set; }

    public ICollection<Category> Categories { get; set; } = new List<Category>();

    public decimal TotalAllocated => Categories?.Sum(c => c.AllocatedAmount) ?? 0;
    public decimal TotalAllocatedInitial => Categories?.Sum(c => c.InitialAllocatedAmount) ?? 0;
    public decimal TotalSpent => Categories?.Sum(c => c.Transactions.Sum(t => t.Amount)) ?? 0;
    public decimal RemainingAmount => TotalAmount - TotalAllocatedInitial;

    [NotMapped]
    public List<Transaction>? RecentTransactions { get; set; }

    public ICollection<Bill> Bills { get; set; } = new List<Bill>(); // Added collection for Bills

    // RowVersion property for concurrency checks
    [Timestamp]
    [ScaffoldColumn(false)] // This prevents RowVersion from being scaffolded in views
    public byte[] RowVersion { get; set; }


}


}
