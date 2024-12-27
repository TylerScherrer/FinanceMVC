using BudgetTracker.Models;

namespace BudgetTracker.Interfaces
{
    public interface IToDoService
    {
        Task<List<ToDoItem>> GetTodayTasksAsync();
        Task<List<ToDoItem>> GetDailyTasksAsync();
        Task<List<DailySchedule>> GetDailySchedulesAsync();
        Task CreateTaskAsync(ToDoItem task);
        Task MarkTaskAsCompleteAsync(int id);
        Task DeleteTaskAsync(int id);
        Task AssignTaskToTimeAsync(int taskId, int hour);
        Task<List<ToDoItem>> GetAllTasksAsync();
Task UnassignTaskAsync(int taskId, int hour);

    }
}
