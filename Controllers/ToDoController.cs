using BudgetTracker.Interfaces;
using BudgetTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Controllers
{
    public class ToDoController : Controller
    {
        private readonly IToDoService _toDoService;

        public ToDoController(IToDoService toDoService)
        {
            _toDoService = toDoService;
        }

        // GET: All tasks
        public async Task<IActionResult> Index()
        {
            var todayTasks = await _toDoService.GetTodayTasksAsync();
            var dailySchedules = await _toDoService.GetDailySchedulesAsync();
            var allTasks = await _toDoService.GetAllTasksAsync(); // Add this line

            var model = new BudgetWithTasksViewModel
            {
                TodayTasks = todayTasks,
                DailySchedules = dailySchedules,
                Budgets = new List<Budget>(), // Empty if not needed
                CurrentWeekTasks = new List<TaskItem>(), // Empty if not needed
                AllTasks = allTasks // Add all tasks here
            };

            return View(model); // Ensure the ToDo/Index view expects BudgetWithTasksViewModel
        }

        // GET: Daily tasks
        public async Task<IActionResult> DailyList()
        {
            var dailyTasks = await _toDoService.GetDailyTasksAsync();
            return View(dailyTasks);
        }

        // GET: Add new task
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ToDoItem task)
        {
            // Log received values
            Console.WriteLine($"Name: {task.Name}, DueDate: {task.DueDate}, IsDaily: {task.IsDaily}, IsTodayOnly: {task.IsTodayOnly}");

            if (ModelState.IsValid)
            {
                await _toDoService.CreateTaskAsync(task);
                return RedirectToAction(nameof(Index));
            }

            // Log model state errors
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                foreach (var error in state.Errors)
                {
                    Console.WriteLine($"Error in {key}: {error.ErrorMessage}");
                }
            }

            return View(task);
        }



        [HttpPost]
        public async Task<IActionResult> MarkComplete(int id)
        {
            await _toDoService.MarkTaskAsCompleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _toDoService.DeleteTaskAsync(id);
            return RedirectToAction(nameof(Index));
        }

[HttpPost]
public async Task<IActionResult> AssignTaskToTime(int taskId, int hour)
{
    // Validate input
    if (taskId <= 0 || hour < 0 || hour > 23)
    {
        TempData["ErrorMessage"] = "Invalid task or hour specified.";
        return RedirectToAction("Index", "Budget");
    }

    try
    {
        // Attempt to assign the task
        await _toDoService.AssignTaskToTimeAsync(taskId, hour);

        // Success feedback
        TempData["SuccessMessage"] = "Task successfully assigned to the selected time.";
    }
    catch (Exception ex)
    {
        // Log the exception (if a logger is available)
        // _logger.LogError(ex, "Error assigning task to time");

        // Error feedback
        TempData["ErrorMessage"] = "An error occurred while assigning the task. Please try again.";
    }

    // Redirect back to the same page
    return RedirectToAction("Index", "Budget");
}

        
        public async Task<List<ToDoItem>> GetAllTasksAsync()
        {
            // Use _toDoService or _context as appropriate
            return await _toDoService.GetAllTasksAsync(); // Use the service
        }
[HttpPost]
public async Task<IActionResult> UnassignTask(int taskId, int hour)
{
    if (taskId <= 0 || hour < 0 || hour > 23)
    {
        ModelState.AddModelError("", "Invalid task or hour specified.");
        return RedirectToAction("Index", "Budget");
    }

    try
    {
        // Attempt to unassign the task
        await _toDoService.UnassignTaskAsync(taskId, hour);

        // Optionally, show a success message
        TempData["SuccessMessage"] = "Task successfully unassigned.";
    }
    catch (Exception ex)
    {
        // Log exception (use a logger if configured)
        // _logger.LogError(ex, "Error unassigning task");

        // Add error feedback for the UI
        TempData["ErrorMessage"] = "An error occurred while unassigning the task.";
    }

    // Redirect back to the appropriate page
    return RedirectToAction("Index", "Budget");
}


[HttpPost]
public async Task<IActionResult> MoveToToday(int taskId)
{
    try
    {
        await _toDoService.MoveTaskToTodayAsync(taskId);
        return RedirectToAction("Index");
    }
    catch (Exception ex)
    {
        // Log or handle errors
        Console.WriteLine($"Error moving task: {ex.Message}");
        ModelState.AddModelError("", ex.Message);
        return RedirectToAction("Index"); // Redirect with errors handled
    }
}




    }
}
