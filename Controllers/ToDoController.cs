using BudgetTracker.Data;
using BudgetTracker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace BudgetTracker.Controllers
{
    public class ToDoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ToDoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: All tasks
        public IActionResult Index()
        {
            var today = DateTime.Now.Date;

            // Tasks for Today: IsDaily OR IsTodayOnly for the current date
            var todayTasks = _context.ToDoItems
                                    .Where(t => t.IsDaily || (t.IsTodayOnly && t.DueDate.Date == today))
                                    .ToList();

            // All tasks
            var allTasks = _context.ToDoItems.ToList();

            var model = new ToDoViewModel
            {
                TodayTasks = todayTasks,
                AllTasks = allTasks
            };

            return View(model);
        }


        // GET: Daily tasks
        public IActionResult DailyList()
        {
            var dailyTasks = _context.ToDoItems
                .Where(t => t.IsDaily)
                .ToList();
            return View(dailyTasks);
        }

        // GET: Add new task
        public IActionResult Create()
        {
            return View();
        }

[HttpPost]
public IActionResult Create(ToDoItem task)
{
    // Debugging: Print all received values
    Console.WriteLine("=== Form Submission Debug ===");
    Console.WriteLine($"Name: {task.Name}");
    Console.WriteLine($"DueDate: {task.DueDate}");
    Console.WriteLine($"IsDaily: {task.IsDaily}");
    Console.WriteLine($"IsTodayOnly: {task.IsTodayOnly}");

    if (ModelState.IsValid)
    {
        _context.ToDoItems.Add(task);
        _context.SaveChanges();
        Console.WriteLine("Task saved successfully!");
        return RedirectToAction(nameof(Index));
    }

    Console.WriteLine("ModelState is not valid. Errors:");
    foreach (var modelStateKey in ModelState.Keys)
    {
        var value = ModelState[modelStateKey];
        foreach (var error in value.Errors)
        {
            Console.WriteLine($"Error in {modelStateKey}: {error.ErrorMessage}");
        }
    }

    return View(task);
}


        // POST: Mark as completed
        [HttpPost]
        public IActionResult MarkComplete(int id)
        {
            var task = _context.ToDoItems.Find(id);
            if (task != null)
            {
                task.IsCompleted = true;
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var task = _context.ToDoItems.Find(id);
            if (task != null)
            {
                _context.ToDoItems.Remove(task);
                _context.SaveChanges();
            }

            // Redirect back to the Index page
            return RedirectToAction(nameof(Index));
        }
[HttpPost]
public IActionResult AssignTaskToTime(int taskId, int hour)
{
    var task = _context.ToDoItems.Find(taskId);

    if (task != null)
    {
        Console.WriteLine($"Task '{task.Name}' assigned to {hour}:00.");
        // Optional: Save this assignment to a database if needed
    }

    return RedirectToAction("Index", "Budget");
}

    }

    
}
