using BudgetTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Interfaces
{
    public interface IScheduleService
    {
        // Retrieves all tasks scheduled for the current week.
        // Returns a list of TaskItem objects representing tasks within the current week.
        Task<List<TaskItem>> GetTasksForCurrentWeekAsync();

        // Retrieves the full schedule, including daily tasks and any associated metadata.
        // Returns a ScheduleViewModel containing the detailed schedule information.
        Task<ScheduleViewModel> GetScheduleAsync();

        // Adds a task to the schedule for a specific date.
        // name: The name of the task to add.
        // date: The date on which the task is scheduled.
        // Returns nothing. Saves the task to the database.
        Task AddTaskAsync(string name, DateTime date);

        // Adds a task to the schedule for a specific date and time.
        // name: The name of the task to add.
        // date: The date on which the task is scheduled.
        // time: The time at which the task is scheduled.
        // Returns nothing. Saves the task to the database.
        Task AddTaskAsync(string name, DateTime date, TimeSpan time);

        // Adds a task spanning multiple dates, starting and ending on specified dates.
        // name: The name of the task to add.
        // startDate: The start date of the task.
        // endDate: The end date of the task.
        // time: The time of day the task should be scheduled.
        // Returns nothing. Saves the task to the database.
        Task AddTaskAsync(string name, DateTime startDate, DateTime endDate, TimeSpan time);

        // Deletes a task from the schedule by its unique identifier.
        // id: The unique identifier of the task to delete.
        // Returns true if the task was successfully deleted, false otherwise.
        Task<bool> DeleteTaskAsync(int id);
    }
}
