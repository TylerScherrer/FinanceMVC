using BudgetTracker.Data;
using BudgetTracker.Interfaces;
using BudgetTracker.Models;

using Microsoft.AspNetCore.Mvc;

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

            // Fetch data using services or context
            var tasks = await _toDoService.GetTasksForDateAsync(selectedDate);

            var model = new BudgetWithTasksViewModel
            {
                TodayTasks = tasks
            };

            return View(model);
        }

public async Task<List<ToDoItem>> GetTasksForDateAsync(DateTime date)
{
    return await _context.ToDoItems
        .Where(t => t.DueDate.Date == date.Date)
        .ToListAsync();
}

public async Task<List<DailySchedule>> GetSchedulesForDateAsync(DateTime date)
{
    return await _context.DailySchedules
        .Include(ds => ds.Task)
        .Where(ds => ds.Task.DueDate.Date == date.Date)
        .ToListAsync();
}

    }
}
