namespace BudgetTracker.Models
{
    public class ScheduleViewModel
    {
        public List<TaskItem> CurrentWeekTasks { get; set; } = new List<TaskItem>();
        public List<TaskItem> UpcomingWeekTasks { get; set; } = new List<TaskItem>();
        public List<TaskItem> FarthestTasks { get; set; } = new List<TaskItem>();

        // Add this property so you can highlight all scheduled dates in your monthly calendar
        public HashSet<DateTime> AllTaskDates
        {
            get
            {
                return CurrentWeekTasks
                    .Concat(UpcomingWeekTasks)
                    .Concat(FarthestTasks)
                    .Select(task => task.Date.Date)
                    .ToHashSet();
            }
        }
    }

    public class TaskItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
    }
}
