using BudgetTracker.Data;
using BudgetTracker.Interfaces;
using BudgetTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Controllers
{
    public class PlannerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IToDoService _toDoService;

        public PlannerController(ApplicationDbContext context, IToDoService toDoService)
        {
            _context = context;
            _toDoService = toDoService;
        }

        public async Task<IActionResult> Index(DateTime? date)
        {
            var selectedDate = date ?? DateTime.Today;

            // Fetch all schedules
            var schedules = await _toDoService.GetAllSchedulesAsync();

            // Group schedules by date
            var tasksByDate = schedules
                .GroupBy(s => s.Date.Date)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Get daily schedules for the selected date
            var dailySchedules = tasksByDate.ContainsKey(selectedDate.Date)
                ? tasksByDate[selectedDate.Date]
                : new List<DailySchedule>();

            var viewModel = new BudgetWithTasksViewModel
            {
                SelectedDate = selectedDate,
                DailySchedules = dailySchedules,
                TodayTasks = await _toDoService.GetTodayTasksAsync(),
                TasksByDate = tasksByDate
            };

            return View(viewModel);
        }

        public async Task<List<ToDoItem>> GetTasksForDateAsync(DateTime date)
        {
            return await _context.ToDoItems
                .Where(t => t.DueDate.HasValue && t.DueDate.Value.Date == date.Date)
                .ToListAsync();
        }

        public async Task<List<DailySchedule>> GetSchedulesForDateAsync(DateTime date)
        {
            return await _context.DailySchedules
                .Include(ds => ds.Task)
                .Where(ds => ds.Task.DueDate.HasValue && ds.Task.DueDate.Value.Date == date.Date)
                .ToListAsync();
        }
    }
}
