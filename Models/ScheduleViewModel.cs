namespace BudgetTracker.Models
{
    public class ScheduleViewModel
    {
        public List<TaskItem> CurrentWeekTasks { get; set; } = new List<TaskItem>();
        public List<TaskItem> UpcomingWeekTasks { get; set; } = new List<TaskItem>();
        public List<TaskItem> FarthestTasks { get; set; } = new List<TaskItem>(); // Tasks beyond the next week
    }

    public class TaskItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
    }
}
