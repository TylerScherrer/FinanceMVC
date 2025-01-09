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
            Console.WriteLine("[DEBUG] Fetching schedule...");
            var schedule = await _scheduleService.GetScheduleAsync();

            // Debugging the retrieved data
            Console.WriteLine($"[DEBUG] Current Week Tasks Count: {schedule.CurrentWeekTasks.Count}");
            Console.WriteLine($"[DEBUG] Upcoming Week Tasks Count: {schedule.UpcomingWeekTasks.Count}");
            Console.WriteLine($"[DEBUG] Farthest Tasks Count: {schedule.FarthestTasks.Count}");

            foreach (var task in schedule.CurrentWeekTasks)
            {
                Console.WriteLine($"[DEBUG] Current Week Task: Id={task.Id}, Name={task.Name}, StartDate={task.StartDate}, EndDate={task.EndDate}, Time={task.Time}");
            }

            foreach (var task in schedule.UpcomingWeekTasks)
            {
                Console.WriteLine($"[DEBUG] Upcoming Week Task: Id={task.Id}, Name={task.Name}, StartDate={task.StartDate}, EndDate={task.EndDate}, Time={task.Time}");
            }

            foreach (var task in schedule.FarthestTasks)
            {
                Console.WriteLine($"[DEBUG] Farthest Task: Id={task.Id}, Name={task.Name}, StartDate={task.StartDate}, EndDate={task.EndDate}, Time={task.Time}");
            }

            return View(schedule);
        }

        [HttpPost]
        public async Task<IActionResult> AddTask(string name, DateTime startDate, DateTime endDate, TimeSpan time)
        {
            Console.WriteLine($"[DEBUG] Adding Task: Name={name}, StartDate={startDate}, EndDate={endDate}, Time={time}");

            if (string.IsNullOrWhiteSpace(name) || startDate > endDate)
            {
                TempData["ErrorMessage"] = "Invalid task details. Please check the name or date range.";
                Console.WriteLine("[DEBUG] Task addition failed due to invalid input.");
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _scheduleService.AddTaskAsync(name, startDate, endDate, time);
                TempData["SuccessMessage"] = "Task(s) added successfully.";
                Console.WriteLine("[DEBUG] Task added successfully.");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to add the task(s): {ex.Message}";
                Console.WriteLine($"[ERROR] Failed to add task: {ex.Message}");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTask(int id)
        {
            Console.WriteLine($"[DEBUG] Deleting Task with Id={id}");

            try
            {
                var success = await _scheduleService.DeleteTaskAsync(id);

                if (!success)
                {
                    Console.WriteLine("[DEBUG] Task not found.");
                    return NotFound("Task not found.");
                }

                Console.WriteLine("[DEBUG] Task deleted successfully.");
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
            Console.WriteLine($"[DEBUG] Deleting Scheduled Task with Id={id}");

            var success = await _scheduleService.DeleteTaskAsync(id);

            if (!success)
            {
                Console.WriteLine("[DEBUG] Task not found.");
                return NotFound("Task not found.");
            }

            Console.WriteLine("[DEBUG] Scheduled task deleted successfully.");
            return RedirectToAction("Index", "Budget");
        }
    }
}
