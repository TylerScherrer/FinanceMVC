using BudgetTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Interfaces
{
    public interface IScheduleService
    {
        Task<List<TaskItem>> GetTasksForCurrentWeekAsync();
        Task<ScheduleViewModel> GetScheduleAsync();

        // Add a task for a single date
        Task AddTaskAsync(string name, DateTime date);

        // Add a task for a single date with time
        Task AddTaskAsync(string name, DateTime date, TimeSpan time);

        // Add a task spanning multiple dates
        Task AddTaskAsync(string name, DateTime startDate, DateTime endDate, TimeSpan time);

        // Delete a task by ID
        Task<bool> DeleteTaskAsync(int id);
    }
}
