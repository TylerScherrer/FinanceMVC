using System.ComponentModel.DataAnnotations;

namespace BudgetTracker.Models
{
    public class ToDoItem
    {
        // The unique identifier for the to-do item.
        public int Id { get; set; }

        // The name or description of the task.
        // This property is required and must not be left empty.
        [Required]
        public string Name { get; set; }

        // The due date for the task.
        // Optional property; defaults to the current date and time if not explicitly set.
        public DateTime? DueDate { get; set; } = DateTime.Now;

        // Indicates whether the task is completed.
        // Defaults to false, meaning the task is not yet completed.
        public bool IsCompleted { get; set; } = false;

        // Indicates whether the task is recurring and should repeat daily.
        // Defaults to false, meaning the task is not a daily task.
        public bool IsDaily { get; set; } = false;

        // Indicates whether the task is assigned specifically for today only.
        // Defaults to false, meaning the task is not restricted to today.
        public bool IsTodayOnly { get; set; } = false;

        // Indicates whether the task is scheduled for today.
        // Can be used to check or set if the task is part of today's schedule.
        public bool IsToday { get; set; }
    }
}
