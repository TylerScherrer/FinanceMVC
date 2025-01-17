using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetTracker.Models;

namespace BudgetTracker.Services
{
    public interface ITaskServiceModel
    {
        // Retrieves a list of all available tasks.
        // This method returns a collection of tasks that are not currently assigned to any schedule.
        // Useful for displaying unassigned tasks to the user for scheduling or management purposes.
        Task<IEnumerable<TaskItem>> GetAvailableTasksAsync();

        // Assigns a task to a specific time slot.
        // Parameters:
        // - `taskId`: The unique identifier of the task to be assigned.
        // - `hour`: The hour (in 24-hour format) for the time slot.
        // - `minute`: The minute for the time slot.
        // This method schedules a task for a specific hour and minute on a selected day.
        Task AssignTaskAsync(int taskId, int hour, int minute);

        // Removes a task from a specific time slot.
        // Parameters:
        // - `taskId`: The unique identifier of the task to be unassigned.
        // - `hour`: The hour (in 24-hour format) from which the task should be removed.
        // - `minute`: The minute from which the task should be removed.
        // This method unassigns a task from its currently scheduled hour and minute.
        Task UnassignTaskAsync(int taskId, int hour, int minute);
    }
}
