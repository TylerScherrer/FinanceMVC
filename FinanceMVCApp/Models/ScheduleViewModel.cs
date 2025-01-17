namespace BudgetTracker.Models
{
    // ViewModel for managing and displaying task schedules.
    // This class organizes tasks into categories based on their proximity to the current date.
    public class ScheduleViewModel
    {
        // Tasks scheduled for the current week.
        public List<TaskItem> CurrentWeekTasks { get; set; } = new List<TaskItem>();

        // Tasks scheduled for the upcoming week.
        public List<TaskItem> UpcomingWeekTasks { get; set; } = new List<TaskItem>();

        // Tasks scheduled for a later time period (beyond the upcoming week).
        public List<TaskItem> FarthestTasks { get; set; } = new List<TaskItem>();





        // A collection of unique dates representing all the task schedules across all categories.
        // This property is used to identify dates that have tasks assigned, typically for highlighting
        // those dates in a calendar view.
        //
        // Explanation:
        // - Combines all tasks from three categories: CurrentWeekTasks, UpcomingWeekTasks, and FarthestTasks.
        // - Calculates individual dates covered by each task's StartDate to EndDate range (inclusive).
        // - Eliminates duplicate dates, ensuring each unique date appears only once in the result.
        //
        // How It Works:
        // 1. Combines tasks from three categories:
        //    - CurrentWeekTasks, UpcomingWeekTasks, and FarthestTasks are merged using `Concat`.
        // 2. Filters out invalid tasks:
        //    - `Where(task => task.EndDate >= task.StartDate)` ensures only tasks with valid date ranges are included.
        // 3. Expands task date ranges:
        //    - For each task, generates all dates from StartDate to EndDate (inclusive):
        //      - `Enumerable.Range(0, (task.EndDate - task.StartDate).Days + 1)`:
        //          - Creates a sequence of numbers representing the number of days between StartDate and EndDate.
        //      - `.Select(offset => task.StartDate.AddDays(offset))`:
        //          - Adds each day offset to StartDate to calculate every date in the range.
        // 4. Consolidates all dates:
        //    - Flattens the expanded date ranges into a single collection using `SelectMany`.
        // 5. Converts to a HashSet:
        //    - `.ToHashSet()` ensures the result contains only unique dates, with duplicates automatically removed.
        //
        // Use Case:
        // - Provides an efficient way to check if a specific date has tasks scheduled by using `AllTaskDates.Contains(date)`.
        // - This is especially useful for applications like calendar views to highlight dates with scheduled tasks.
        public HashSet<DateTime> AllTaskDates =>
            CurrentWeekTasks
            .Concat(UpcomingWeekTasks) // Combine tasks from current and upcoming weeks.
            .Concat(FarthestTasks)     // Add tasks from the farthest category.
            .Where(task => task.EndDate >= task.StartDate) // Ensure the date range is valid.
            .SelectMany(task => Enumerable.Range(0, (task.EndDate - task.StartDate).Days + 1)
                .Select(offset => task.StartDate.AddDays(offset))) // Generate all dates in the range.
            .ToHashSet(); // Convert the collection to a HashSet to remove duplicates.
            }


    // Represents a single task with associated details like name, dates, and time.
    public class TaskItem
    {
        // Unique identifier for the task.
        public int Id { get; set; }

        // The name or description of the task.
        public string Name { get; set; }

        // The start date of the task.
        public DateTime StartDate { get; set; }

        // The end date of the task.
        public DateTime EndDate { get; set; }

        // The specific time associated with the task.
        public TimeSpan Time { get; set; }
    }
}
