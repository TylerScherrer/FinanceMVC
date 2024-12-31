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

            // Get tasks for the current week
            var currentWeekTasks = await _context.Tasks
                .Where(t => t.Date >= currentDate.StartOfWeek() && t.Date <= currentDate.EndOfWeek())
                .ToListAsync();

            // Get tasks for the upcoming week
            var upcomingWeekTasks = await _context.Tasks
                .Where(t => t.Date > currentDate.EndOfWeek() && t.Date <= currentDate.AddDays(14).EndOfWeek())
                .ToListAsync();

            // Get tasks beyond the next two weeks
            var farthestTasks = await _context.Tasks
                .Where(t => t.Date > currentDate.AddDays(14).EndOfWeek())
                .ToListAsync();

            // Create and return the schedule view model
            return new ScheduleViewModel
            {
                CurrentWeekTasks = currentWeekTasks,
                UpcomingWeekTasks = upcomingWeekTasks,
                FarthestTasks = farthestTasks
            };
        }

        public async Task AddTaskAsync(string name, DateTime date)
        {
            if (!string.IsNullOrEmpty(name) && date != default)
            {
                var newTask = new TaskItem
                {
                    Name = name,
                    Date = date
                };

                _context.Tasks.Add(newTask);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<List<TaskItem>> GetTasksForCurrentWeekAsync()
        {
            var currentDate = DateTime.Now;

            // Calculate the start and end of the current week
            var startOfWeek = currentDate.StartOfWeek();
            var endOfWeek = currentDate.EndOfWeek();

            // Fetch tasks for the current week
            return await _context.Tasks
                .Where(t => t.Date >= startOfWeek && t.Date <= endOfWeek)
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

public async Task AddTaskAsync(string name, DateTime date, TimeSpan time)
{
    var newTask = new TaskItem
    {
        Name = name,
        Date = date,
        Time = time
    };

    _context.Tasks.Add(newTask);
    await _context.SaveChangesAsync();
}


    }
}
