using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetTracker.Models;

namespace BudgetTracker.Services
{
    public interface ITaskServiceModel
    {
        Task<IEnumerable<TaskItem>> GetAvailableTasksAsync();
        Task AssignTaskAsync(int taskId, int hour, int minute);
        Task UnassignTaskAsync(int taskId, int hour, int minute);
    }
}
