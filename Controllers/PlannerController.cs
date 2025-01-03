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

    // Fetch today's tasks
    var todayTasks = await _toDoService.GetTodayTasksAsync();

    // Fetch schedules for the selected date
    var dailySchedules = await _toDoService.GetSchedulesForDateAsync(selectedDate);

    var model = new BudgetWithTasksViewModel
    {
        TodayTasks = todayTasks, // List of tasks assigned for today
        DailySchedules = dailySchedules, // Schedules for the day
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
