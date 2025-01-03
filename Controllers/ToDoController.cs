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
public async Task<IActionResult> Index(DateTime? date)
{
    var selectedDate = date ?? DateTime.Today;

    // Fetch today's tasks
    var todayTasks = await _toDoService.GetTodayTasksAsync();

    // Fetch all tasks
    var allTasks = await _toDoService.GetAllTasksAsync();

    var viewModel = new BudgetWithTasksViewModel
    {
        SelectedDate = selectedDate,
        TodayTasks = todayTasks,
        AllTasks = allTasks,
        DailySchedules = new List<DailySchedule>(), // Add schedules if needed
    };

    return View(viewModel);
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
    Console.WriteLine($"Name: {task.Name}, DueDate: {task.DueDate}, IsDaily: {task.IsDaily}, IsTodayOnly: {task.IsTodayOnly}");

    if (ModelState.IsValid)
    {
        if (task.IsTodayOnly)
        {
            task.IsToday = true; // Mark it for today only
            task.DueDate = null; // Remove due date since it's today-specific
        }
        else if (task.DueDate.HasValue)
        {
            task.IsToday = task.DueDate.Value.Date == DateTime.Today;
        }

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
public async Task<IActionResult> AssignTaskToTime(int? taskId, string newTaskName, int hour, int minute, DateTime selectedDate)
{
    Console.WriteLine($"AssignTaskToTime called with: taskId={taskId}, newTaskName={newTaskName}, hour={hour}, minute={minute}, date={selectedDate}");

    // If user typed a new task name, create that task.
    int newTaskId = 0;
    if (!string.IsNullOrWhiteSpace(newTaskName))
    {
        // Create a new ToDoItem
        var newTask = new ToDoItem
        {
            Name = newTaskName,
            DueDate = selectedDate, 
            IsCompleted = false
            // ... set other fields as you see fit
        };

        // Save it (using your IToDoService or context)
        // Example:
        await _toDoService.CreateTaskAsync(newTask);
        newTaskId = newTask.Id; // After saving, you should have the new ID
        Console.WriteLine($"[DEBUG] Created a new Task with ID {newTask.Id} for name '{newTask.Name}'");
    }

    // If we got a newTaskId from above, use that; otherwise, use the existing taskId
    int finalTaskId = (newTaskId > 0) ? newTaskId : (taskId ?? 0);

    // Validate
    if (finalTaskId <= 0)
    {
        Console.WriteLine("[ERROR] No valid task ID found, and no new task name entered.");
        TempData["ErrorMessage"] = "Please select an existing task or enter a new task name.";
        return RedirectToAction("Index", "Planner", new { date = selectedDate });
    }

    // Actually assign the task
    try
    {
        await _toDoService.AssignTaskToTimeAsync(finalTaskId, hour, selectedDate, minute);
        Console.WriteLine($"[DEBUG] Assigned Task {finalTaskId} at {hour}:{minute:D2} on {selectedDate.ToShortDateString()}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] Failed to assign task: {ex.Message}");
        TempData["ErrorMessage"] = "Failed to assign task. " + ex.Message;
    }

    // Redirect back to the Planner
    return RedirectToAction("Index", "Planner", new { date = selectedDate });
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
        return RedirectToAction("Index", "Planner");
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
    return RedirectToAction("Index", "Planner");
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
