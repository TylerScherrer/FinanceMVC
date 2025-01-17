using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.Models
{
    // Represents a daily schedule entry for a task, including time and date information.
    public class DailySchedule
    {
        // The unique identifier for the daily schedule entry.
        public int Id { get; set; }

        // The ID of the associated task. This serves as a foreign key linking to the ToDoItem.
        [Required]
        public int TaskId { get; set; } // Foreign Key to ToDoItem

        // The hour for the scheduled task (e.g., 9, 10). Represents the hour of the day.
        [Required]
        public int Hour { get; set; } // Time slot (e.g., 9, 10, etc.)

        // Navigation property for the associated task. 
        // Allows EF Core to establish a relationship between the DailySchedule and ToDoItem models.
        public ToDoItem Task { get; set; } // Navigation Property

        // The minute of the scheduled task (e.g., 15, 30). Allows for more precise scheduling.
        public int Minute { get; set; } // New property for minutes

        // The date for the scheduled task. Represents the specific day the task is assigned to.
        public DateTime Date { get; set; } // New property
    }
}
