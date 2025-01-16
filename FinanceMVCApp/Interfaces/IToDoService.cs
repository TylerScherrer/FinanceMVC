using BudgetTracker.Models;

namespace BudgetTracker.Interfaces
{
    public interface IToDoService
    {
        // Retrieves a list of tasks scheduled for today.
        // Returns a list of ToDoItem objects representing today's tasks.
        Task<List<ToDoItem>> GetTodayTasksAsync();

        // Retrieves all daily tasks across the schedule.
        // Returns a list of ToDoItem objects representing daily tasks.
        Task<List<ToDoItem>> GetDailyTasksAsync();

        // Retrieves all daily schedules.
        // Returns a list of DailySchedule objects containing scheduling details for each day.
        Task<List<DailySchedule>> GetDailySchedulesAsync();

        // Creates a new task in the system.
        // task: The ToDoItem object representing the task to be created.
        // Saves the task to the database.
        Task CreateTaskAsync(ToDoItem task);

        // Marks a specific task as completed.
        // id: The unique identifier of the task to mark as complete.
        Task MarkTaskAsCompleteAsync(int id);

        // Deletes a specific task by its unique identifier.
        // id: The unique identifier of the task to delete.
        Task DeleteTaskAsync(int id);

        // Assigns a task to a specific time on a specific date.
        // taskId: The unique identifier of the task to assign.
        // hour: The hour (in 24-hour format) when the task should occur.
        // date: The date when the task is scheduled.
        // minute: The minute of the hour for the scheduled task.
        Task AssignTaskToTimeAsync(int taskId, int hour, DateTime date, int minute);

        // Retrieves all tasks in the system.
        // Returns a list of ToDoItem objects representing all tasks.
        Task<List<ToDoItem>> GetAllTasksAsync();

        // Unassigns a task from a specific hour of the day.
        // taskId: The unique identifier of the task to unassign.
        // hour: The hour from which the task should be unassigned.
        Task UnassignTaskAsync(int taskId, int hour);

        // Moves a task to today's schedule.
        // taskId: The unique identifier of the task to move to today.
        Task MoveTaskToTodayAsync(int taskId);

        // Retrieves tasks scheduled for a specific date.
        // date: The date for which to retrieve tasks.
        // Returns a list of ToDoItem objects scheduled for the specified date.
        Task<List<ToDoItem>> GetTasksForDateAsync(DateTime date);

        // Retrieves daily schedules for a specific date.
        // date: The date for which to retrieve schedules.
        // Returns a list of DailySchedule objects for the specified date.
        Task<List<DailySchedule>> GetSchedulesForDateAsync(DateTime date);

        // Retrieves all schedules in the system.
        // Returns a list of DailySchedule objects representing all schedules.
        Task<List<DailySchedule>> GetAllSchedulesAsync();
    }
}
