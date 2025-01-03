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
        Task AssignTaskToTimeAsync(int taskId, int hour, DateTime date);
        Task<List<ToDoItem>> GetAllTasksAsync();
Task UnassignTaskAsync(int taskId, int hour);
Task MoveTaskToTodayAsync(int taskId);

        Task<List<ToDoItem>> GetTasksForDateAsync(DateTime date);
        Task<List<DailySchedule>> GetSchedulesForDateAsync(DateTime date);

        Task<List<DailySchedule>> GetAllSchedulesAsync();

    }
}
