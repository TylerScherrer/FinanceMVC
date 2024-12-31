using BudgetTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Interfaces
{
    public interface IScheduleService
    {
         Task<List<TaskItem>> GetTasksForCurrentWeekAsync();
        Task<ScheduleViewModel> GetScheduleAsync();
        Task AddTaskAsync(string name, DateTime date);
        Task<bool> DeleteTaskAsync(int id);

       Task AddTaskAsync(string name, DateTime date, TimeSpan time); // Include time parameter
    }
}
