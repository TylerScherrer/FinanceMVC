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











        // A collection of unique dates that represent all task schedules across all categories.
        // This property consolidates tasks from the current week, upcoming week, and farthest future tasks,
        // and calculates the individual dates covered by each task's start and end date range.
        //
        // How It Works:
        // 1. Combines all tasks from three categories: CurrentWeekTasks, UpcomingWeekTasks, and FarthestTasks.
        //    - `Concat(CurrentWeekTasks)` merges the current week's tasks with upcoming and farthest tasks.
        // 2. Filters out tasks with invalid date ranges where the EndDate is earlier than the StartDate.
        //    - `Where(task => task.EndDate >= task.StartDate)` ensures only valid tasks are processed.
        // 3. For each task, generates all dates from its StartDate to EndDate (inclusive):
        //    - `Enumerable.Range(0, (task.EndDate - task.StartDate).Days + 1)`:
        //        - Creates a range of numbers representing the offset in days from the StartDate.
        //    - `.Select(offset => task.StartDate.AddDays(offset))`:
        //        - Adds the offset to the StartDate to calculate each individual date in the range.
        // 4. Flattens all the generated date ranges into a single collection using `SelectMany`.
        //    - This ensures all dates from all tasks are included in one list.
        // 5. Converts the final collection to a `HashSet<DateTime>` using `.ToHashSet()`:
        //    - Eliminates duplicate dates so that each date appears only once in the result.
        //
        // Benefits:
        // - Ensures no duplicate dates.
        // - Provides efficient lookups to check if a specific date is included in the set.
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
