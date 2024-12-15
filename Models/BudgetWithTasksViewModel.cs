namespace BudgetTracker.Models
{
    public class BudgetWithTasksViewModel
    {
        public List<Budget> Budgets { get; set; }
        public List<TaskItem> CurrentWeekTasks { get; set; }
    }
}
