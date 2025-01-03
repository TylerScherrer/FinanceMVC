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
            // Handle null date with default value
            var selectedDate = date?.Date ?? DateTime.Today; // Use ?.Date to handle nullable DateTime correctly

            // Fetch tasks and schedules for the selected date
            var tasks = await _toDoService.GetTasksForDateAsync(selectedDate);
            var schedules = await _toDoService.GetSchedulesForDateAsync(selectedDate);

            // Initialize the view model
            var model = new BudgetWithTasksViewModel
            {
                TodayTasks = tasks ?? new List<ToDoItem>(),
                DailySchedules = schedules ?? new List<DailySchedule>(),
                SelectedDate = selectedDate
            };

            return View(model);
        }





public async Task<List<ToDoItem>> GetTasksForDateAsync(DateTime date)
{
    return await _context.ToDoItems
        .Where(t => t.DueDate.HasValue && t.DueDate.Value.Date == date.Date) // Handle nullable DateTime
        .ToListAsync();
}

public async Task<List<DailySchedule>> GetSchedulesForDateAsync(DateTime date)
{
    return await _context.DailySchedules
        .Include(ds => ds.Task)
        .Where(ds => ds.Task.DueDate.HasValue && ds.Task.DueDate.Value.Date == date.Date) // Handle nullable DateTime
        .ToListAsync();
}


    }
}
