namespace BudgetTracker.Models
{

    /// ViewModel for combining budget-related data with tasks and schedules.
    /// This class serves as a container to encapsulate data needed for views that 
    /// display budgets, tasks, schedules, and bills together.
    public class BudgetWithTasksViewModel
    {

        /// A list of all budgets available in the system.
        /// Used for displaying or selecting budgets in the UI.
        public List<Budget> Budgets { get; set; } = new List<Budget>();

        /// Tasks scheduled for the current week.
        /// Provides a filtered view of tasks relevant to the ongoing week.
        public List<TaskItem> CurrentWeekTasks { get; set; } = new List<TaskItem>();

        /// Tasks specifically scheduled for today.
        /// Provides a quick overview of tasks that are due or planned for the current day.
        public List<ToDoItem> TodayTasks { get; set; } = new List<ToDoItem>();

        /// A collection of daily schedules.
        /// These schedules group tasks by specific dates, allowing for structured organization.
        public List<DailySchedule> DailySchedules { get; set; } = new List<DailySchedule>();


        /// A comprehensive list of all tasks available.
        /// Provides access to all tasks without any filters.
        public List<ToDoItem> AllTasks { get; set; } = new List<ToDoItem>();


        /// A list of monthly bills associated with budgets.
        /// Allows tracking and displaying of recurring or scheduled payments.
        public List<Bill> MonthlyBills { get; set; }

        /// The currently selected date.
        /// Useful for displaying or filtering tasks and schedules based on user interaction.
        public DateTime SelectedDate { get; set; }

   
        /// A dictionary grouping tasks by their associated dates.
        /// The keys represent dates, and the values are lists of tasks scheduled for those dates.
        public Dictionary<DateTime, List<DailySchedule>> TasksByDate { get; set; } = new Dictionary<DateTime, List<DailySchedule>>();


        /// Retrieves a list of unique dates as formatted strings from the grouped tasks.
        /// The dates are formatted as "yyyy-MM-dd" for consistency and display purposes.
        /// <returns>A list of date strings in "yyyy-MM-dd" format.</returns>
        public List<string> GetTaskDates()
        {
            return TasksByDate.Keys.Select(date => date.ToString("yyyy-MM-dd")).ToList();
        }
    }
}
