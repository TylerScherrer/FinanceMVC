namespace BudgetTracker.Models
{
    public class ToDoViewModel
    {
        public List<ToDoItem> TodayTasks { get; set; }
        public List<ToDoItem> AllTasks { get; set; }
        public List<DailySchedule> DailySchedules { get; set; } // Task assignments
    }
}
