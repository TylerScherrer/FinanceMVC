using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.Models
{
    public class DailySchedule
    {
        public int Id { get; set; }

        [Required]
        public int TaskId { get; set; } // Foreign Key to ToDoItem

        [Required]
        public int Hour { get; set; } // Time slot (e.g., 9, 10, etc.)

        public ToDoItem Task { get; set; } // Navigation Property

        public int Minute { get; set; } // New property for minutes
    }
}
