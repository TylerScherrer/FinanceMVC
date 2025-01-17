namespace BudgetTracker.Models
{
    public class ToDoViewModel
    {
        // A list of tasks specifically assigned for today.
        // This collection contains only those tasks that are relevant for the current date.
        public List<ToDoItem> TodayTasks { get; set; }

        // A list of all tasks, regardless of their due date or status.
        // This collection provides a complete overview of all tasks in the system.
        public List<ToDoItem> AllTasks { get; set; }

        // A list of daily schedules, which represent tasks assigned to specific time slots or recurring schedules.
        // This collection helps organize task assignments within defined schedules.
        public List<DailySchedule> DailySchedules { get; set; }
    }
}
