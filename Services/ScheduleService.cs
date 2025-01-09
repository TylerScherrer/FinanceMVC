using BudgetTracker.Data;
using BudgetTracker.Interfaces;
using BudgetTracker.Models;
using BudgetTracker.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly ApplicationDbContext _context;

        public ScheduleService(ApplicationDbContext context)
        {
            _context = context;
        }

public async Task<ScheduleViewModel> GetScheduleAsync()
{
    var currentDate = DateTime.Now;

    // Fetch all tasks from the database
    var allTasks = await _context.Tasks.ToListAsync();

    // Remove duplicates by grouping by task ID
    allTasks = allTasks.GroupBy(t => t.Id)
                       .Select(g => g.First())
                       .ToList();

    var startOfWeek = currentDate.StartOfWeek();
    var endOfWeek = currentDate.EndOfWeek();

    // Get tasks for the current week (including overlapping tasks)
    var currentWeekTasks = allTasks
        .Where(t => t.StartDate <= endOfWeek && t.EndDate >= startOfWeek)
        .ToList();

    // Get tasks for the upcoming week (next 7 days)
    var upcomingWeekTasks = allTasks
        .Where(t => t.StartDate > endOfWeek && t.StartDate <= endOfWeek.AddDays(7))
        .ToList();

    // Get tasks beyond the next two weeks
    var farthestTasks = allTasks
        .Where(t => t.StartDate > endOfWeek.AddDays(7))
        .ToList();

    return new ScheduleViewModel
    {
        CurrentWeekTasks = currentWeekTasks,
        UpcomingWeekTasks = upcomingWeekTasks,
        FarthestTasks = farthestTasks
    };
}




        // Multi-day task
// Multi-day task
// Multi-day task
public async Task AddTaskAsync(string name, DateTime startDate, DateTime endDate, TimeSpan time)
{
    if (startDate > endDate)
        throw new ArgumentException("StartDate cannot be later than EndDate.");

    // Add a single task with the full date range
    var newTask = new TaskItem
    {
        Name = name,
        StartDate = startDate,
        EndDate = endDate,
        Time = time
    };

    _context.Tasks.Add(newTask);
    await _context.SaveChangesAsync();
}




        // Single-day task with date and time
        public async Task AddTaskAsync(string name, DateTime date, TimeSpan time)
        {
            var newTask = new TaskItem
            {
                Name = name,
                StartDate = date,
                EndDate = date, // Single-day task has the same start and end date
                Time = time
            };

            _context.Tasks.Add(newTask);
            await _context.SaveChangesAsync();
        }

        // Single-day task with only date
        public async Task AddTaskAsync(string name, DateTime date)
        {
            
            var newTask = new TaskItem
            {
                Name = name,
                StartDate = date,
                EndDate = date // Single-day task
            };

            _context.Tasks.Add(newTask);
            await _context.SaveChangesAsync();
        }

        public async Task<List<TaskItem>> GetTasksForCurrentWeekAsync()
        {
            var currentDate = DateTime.Now;

            // Calculate the start and end of the current week
            var startOfWeek = currentDate.StartOfWeek();
            var endOfWeek = currentDate.EndOfWeek();

            // Fetch tasks for the current week
            return await _context.Tasks
                .Where(t => t.StartDate <= endOfWeek && t.EndDate >= startOfWeek)
                .ToListAsync();
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return false; // Task not found
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true; // Task successfully deleted
        }
    }
}
