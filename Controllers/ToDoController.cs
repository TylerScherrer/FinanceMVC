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
            await _toDoService.AssignTaskToTimeAsync(taskId, hour);
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
    try
    {
        await _toDoService.UnassignTaskAsync(taskId, hour);
        return RedirectToAction("Index", "Budget"); // Redirect back to the same page
    }
    catch (Exception ex)
    {
        ModelState.AddModelError("", ex.Message);
        return RedirectToAction(nameof(Index)); // Optionally add error handling UI
    }
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
