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

    public decimal RemainingAmount => TotalAmount - (Categories?.Sum(c => c.AllocatedAmount) ?? 0);

    [NotMapped]
    public List<Transaction>? RecentTransactions { get; set; }


}


}
