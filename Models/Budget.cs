using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.Models
{
    public class Budget
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.Now;

        // Collection of Categories
        public List<Category> Categories { get; set; } = new List<Category>();
    }
}
