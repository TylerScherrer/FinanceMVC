namespace BudgetTracker.Models
{
public class ScheduleViewModel
{
    public List<TaskItem> CurrentWeekTasks { get; set; } = new List<TaskItem>();
    public List<TaskItem> UpcomingWeekTasks { get; set; } = new List<TaskItem>();
    public List<TaskItem> FarthestTasks { get; set; } = new List<TaskItem>();

public HashSet<DateTime> AllTaskDates =>
    CurrentWeekTasks
    .Concat(UpcomingWeekTasks)
    .Concat(FarthestTasks)
    .Where(task => task.EndDate >= task.StartDate) // Ensure valid date range
    .SelectMany(task => Enumerable.Range(0, (task.EndDate - task.StartDate).Days + 1)
        .Select(offset => task.StartDate.AddDays(offset)))
    .ToHashSet();

}

    public class TaskItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; } // Start of the task
        public DateTime EndDate { get; set; }   // End of the task
        public TimeSpan Time { get; set; }
    }
}
