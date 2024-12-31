using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.Models
{
    public class ToDoItem
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } // Task name or description

        [Required]
        public DateTime DueDate { get; set; } = DateTime.Now; // Add this property

        public bool IsCompleted { get; set; } = false; // Task status
        public bool IsDaily { get; set; } = false;    // Recurring daily tasks
        public bool IsTodayOnly { get; set; } = false; // Tasks assigned only for today

        public bool IsToday { get; set; } // Add this property if it doesn't already exist

        
    }
}
