using Microsoft.AspNetCore.Mvc;
using BudgetTracker.Interfaces;
using BudgetTracker.Models;

namespace BudgetTracker.Controllers
{
    public class ScheduleController : Controller
    {
        private readonly IScheduleService _scheduleService;

        public ScheduleController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        public async Task<IActionResult> Index()
        {
            var schedule = await _scheduleService.GetScheduleAsync();
            return View(schedule);
        }

[HttpPost]
public async Task<IActionResult> AddTask(string Name, DateTime Date, TimeSpan Time)
{
    Console.WriteLine($"[INFO] AddTask called with Name: {Name}, Date: {Date}, Time: {Time}");

    if (string.IsNullOrWhiteSpace(Name))
    {
        Console.WriteLine("[ERROR] Task name is null or empty.");
        TempData["ErrorMessage"] = "Task name cannot be empty.";
        return RedirectToAction(nameof(Index));
    }

    try
    {
        await _scheduleService.AddTaskAsync(Name, Date, Time);
        Console.WriteLine($"[INFO] Task '{Name}' added for {Date} at {Time}.");
        TempData["SuccessMessage"] = "Task added successfully.";
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] Failed to add task: {ex.Message}");
        TempData["ErrorMessage"] = "Failed to add the task. Please try again.";
    }

    return RedirectToAction(nameof(Index));
}




        [HttpPost]
        public async Task<IActionResult> DeleteTask(int id)
        {
            try
            {
                var success = await _scheduleService.DeleteTaskAsync(id);

                if (!success)
                {
                    return NotFound("Task not found.");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to delete task: {ex.Message}");
                return BadRequest("An error occurred while deleting the task.");
            }
        }
        [HttpPost]
        public async Task<IActionResult> DeleteScheduledTask(int id)
        {
            var success = await _scheduleService.DeleteTaskAsync(id);

            if (!success)
                return NotFound("Task not found.");

            return RedirectToAction("Index", "Budget");
        }


    }
}
