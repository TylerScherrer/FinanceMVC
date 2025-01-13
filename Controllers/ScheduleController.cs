using Microsoft.AspNetCore.Mvc;
using BudgetTracker.Interfaces;
using BudgetTracker.Models;

namespace BudgetTracker.Controllers
{
    // ***********
    // ScheduleController
    // ***********

    // The ScheduleController handles the operations related to schedules,
    // including displaying schedules (Index) and other CRUD functionalities as needed.
    // It relies on the IScheduleService to fetch or manipulate schedule-related data.
    public class ScheduleController : Controller
    {
        // ***********
        // Private Field
        // ***********

        // Holds the reference to the IScheduleService instance, provided via dependency injection.
        private readonly IScheduleService _scheduleService;

        // ***********
        // Constructor
        // ***********

        // Initializes the ScheduleController with the required service.
        // Dependency injection ensures that the controller is loosely coupled to the service.
        public ScheduleController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService; // Assign the provided service to the private field.
        }

        // ***********
        // Index Method
        // ***********

        // The Index method serves as the default action for the ScheduleController.
        // It retrieves the complete schedule from the service and passes it to the Index view.
        // This method is asynchronous to ensure server responsiveness.
        public async Task<IActionResult> Index()
        {

            // Use the IScheduleService to fetch the schedule data asynchronously.
            var schedule = await _scheduleService.GetScheduleAsync();

            // Pass the retrieved schedule data to the Index view for rendering.
            return View(schedule);
        }
    




    // ***********
    // POST Method to Add a Task
    // ***********

    // This method handles the addition of new tasks to the schedule.
    // It validates the input parameters and uses the schedule service to add the task(s).
    [HttpPost]
    public async Task<IActionResult> AddTask(string name, DateTime startDate, DateTime endDate, TimeSpan time)
    {
        // Validate input parameters.
        if (string.IsNullOrWhiteSpace(name) || startDate > endDate)
        {
            // Set an error message in TempData for the user if validation fails.
            TempData["ErrorMessage"] = "Invalid task details. Please check the name or date range.";
            return RedirectToAction(nameof(Index)); // Redirect to the Index page.
        }

        try
        {
            // Use the schedule service to add the task asynchronously.
            await _scheduleService.AddTaskAsync(name, startDate, endDate, time);

            // Set a success message in TempData for the user.
            TempData["SuccessMessage"] = "Task(s) added successfully.";
        }
        catch (Exception ex)
        {
            // Set an error message in TempData if an exception occurs.
            TempData["ErrorMessage"] = $"Failed to add the task(s): {ex.Message}";
        }

        // Redirect to the Index page regardless of success or failure.
        return RedirectToAction(nameof(Index));
    }





    // ***********
    // POST Method to Delete a Task
    // ***********

    // This method handles the deletion of a specific task based on its ID.
    // It uses the schedule service to attempt deletion and returns appropriate responses based on the outcome.

    [HttpPost]
    public async Task<IActionResult> DeleteTask(int id)
    {
        try
        {
            // Attempt to delete the task using the schedule service.
            var success = await _scheduleService.DeleteTaskAsync(id);

            // If the task was not found, return a 404 Not Found response.
            if (!success)
            {
                return NotFound("Task not found.");
            }

            // Redirect to the Index page if the deletion is successful.
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            // Return a 400 Bad Request response if an error occurs during deletion.
            return BadRequest("An error occurred while deleting the task.");
        }
    }





    // ***********
    // POST Method to Delete a Scheduled Task
    // ***********

    // This method deletes a specific scheduled task by its ID.
    // It uses the schedule service to attempt deletion and redirects the user to the budget page if successful.

    [HttpPost]
    public async Task<IActionResult> DeleteScheduledTask(int id)
    {
        // Attempt to delete the task using the schedule service.
        var success = await _scheduleService.DeleteTaskAsync(id);

        // If the task was not found, return a 404 Not Found response.
        if (!success)
        {
            return NotFound("Task not found.");
        }

        // Redirect to the Index action of the BudgetController if deletion is successful.
        return RedirectToAction("Index", "Budget");
    }







    // End of Controller 

    }
}
