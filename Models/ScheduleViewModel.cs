namespace BudgetTracker.Models
{
    public class ScheduleViewModel
    {
        public List<TaskItem> CurrentWeekTasks { get; set; }
        public List<TaskItem> UpcomingWeekTasks { get; set; }
    }

    public class TaskItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
    }
}