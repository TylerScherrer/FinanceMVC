using BudgetTracker.Models;

namespace BudgetTracker.Interfaces
{
    public interface IScheduleService
    {
         Task<List<TaskItem>> GetTasksForCurrentWeekAsync();
        Task<ScheduleViewModel> GetScheduleAsync();
        Task AddTaskAsync(string name, DateTime date);
    }
}
