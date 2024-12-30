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
        public async Task<IActionResult> AddTask(string name, DateTime date)
        {
            await _scheduleService.AddTaskAsync(name, date);
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


    }
}
