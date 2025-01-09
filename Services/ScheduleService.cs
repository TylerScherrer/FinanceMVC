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
                .Where(t => t.StartDate <= currentDate.EndOfWeek() && t.EndDate >= currentDate.StartOfWeek())
                .ToListAsync();

            // Get tasks for the upcoming week
            var upcomingWeekTasks = await _context.Tasks
                .Where(t => t.StartDate > currentDate.EndOfWeek() && t.StartDate <= currentDate.AddDays(14).EndOfWeek())
                .ToListAsync();

            // Get tasks beyond the next two weeks
            var farthestTasks = await _context.Tasks
                .Where(t => t.StartDate > currentDate.AddDays(14).EndOfWeek())
                .ToListAsync();

            // Create and return the schedule view model
            return new ScheduleViewModel
            {
                CurrentWeekTasks = currentWeekTasks,
                UpcomingWeekTasks = upcomingWeekTasks,
                FarthestTasks = farthestTasks
            };
        }

        // Multi-day task
        public async Task AddTaskAsync(string name, DateTime startDate, DateTime endDate, TimeSpan time)
        {
            if (startDate > endDate)
{
    throw new ArgumentException("StartDate cannot be later than EndDate.");
}

            if (startDate > endDate)
            {
                throw new ArgumentException("Start date cannot be after the end date.");
            }

            // Add tasks for each day in the range
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var newTask = new TaskItem
                {
                    Name = name,
                    StartDate = startDate,
                    EndDate = endDate,
                    Time = time
                };

                _context.Tasks.Add(newTask);
            }

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
