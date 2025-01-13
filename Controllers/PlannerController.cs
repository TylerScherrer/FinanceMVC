using BudgetTracker.Data;
using BudgetTracker.Interfaces;
using BudgetTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Controllers
{
// ***********
// PlannerController
// ***********

// The PlannerController handles requests related to planning and managing tasks.
// It interacts with the database and the To-Do service to retrieve and manipulate planning data.
// Inherits from the base Controller class, which provides features for handling HTTP requests and responses.

public class PlannerController : Controller
{
    // ***********
    // Private Fields
    // ***********

    // ApplicationDbContext provides access to the database.
    // Allows the controller to query and modify database entities directly.
    private readonly ApplicationDbContext _context;

    // IToDoService is a service interface for managing To-Do tasks.
    // Encapsulates business logic for handling task-related operations.
    private readonly IToDoService _toDoService;

    // ***********
    // Constructor
    // ***********

    // Initializes the PlannerController with the necessary dependencies via dependency injection.
    // Dependencies:
    // - ApplicationDbContext: Used for direct database access.
    // - IToDoService: Encapsulates task management logic and decouples the controller from specific implementations.
    public PlannerController(ApplicationDbContext context, IToDoService toDoService)
    {
        // Assign the provided ApplicationDbContext instance to the private _context field.
        // This allows the controller to interact with the database.
        _context = context;

        // Assign the provided IToDoService instance to the private _toDoService field.
        // This enables the controller to use To-Do service methods for task management.
        _toDoService = toDoService;
    }




    // ***********
    // Index Method
    // ***********

    // The Index method serves as the default action for the PlannerController.
    // It displays a list of schedules and tasks grouped by date, with an optional date filter.
    // If no date is provided, it defaults to today's date.

    public async Task<IActionResult> Index(DateTime? date)
    {
        // ***********
        // Variables and Initialization
        // ***********

        // Determine the selected date. If no date is provided, use today's date.
        var selectedDate = date ?? DateTime.Today;

        // Fetch all schedules from the To-Do service.
        var schedules = await _toDoService.GetAllSchedulesAsync();

        // Group schedules by their date property into a dictionary.
        // The dictionary keys are the dates, and the values are lists of schedules for each date.
        var tasksByDate = schedules
            .GroupBy(s => s.Date.Date) // Group schedules by their date.
            .ToDictionary(g => g.Key, g => g.ToList()); // Convert the groupings into a dictionary.

        // Retrieve the daily schedules for the selected date.
        // If there are no schedules for the selected date, return an empty list.
        var dailySchedules = tasksByDate.ContainsKey(selectedDate.Date)
            ? tasksByDate[selectedDate.Date] // Get schedules for the selected date.
            : new List<DailySchedule>(); // Return an empty list if no schedules exist.

        // ***********
        // ViewModel Creation
        // ***********

        // Create a ViewModel to combine all the necessary data for the view.
        var viewModel = new BudgetWithTasksViewModel
        {
            SelectedDate = selectedDate,                     // The selected date for filtering.
            DailySchedules = dailySchedules,                // The daily schedules for the selected date.
            TodayTasks = await _toDoService.GetTodayTasksAsync(), // Tasks for today fetched from the service.
            TasksByDate = tasksByDate                        // All schedules grouped by date.
        };

        // Pass the ViewModel to the view for rendering.
        return View(viewModel);
    }





    // ***********
    // GetTasksForDateAsync Method
    // ***********

    // This method retrieves a list of To-Do items that are due on a specific date.
    // It queries the database for tasks with a DueDate matching the provided date.
    public async Task<List<ToDoItem>> GetTasksForDateAsync(DateTime date)
    {
        // Query the database for tasks where the DueDate matches the provided date.
        // Only tasks with a non-null DueDate are considered.
        return await _context.ToDoItems
            .Where(t => t.DueDate.HasValue && t.DueDate.Value.Date == date.Date)
            .ToListAsync();
    }






    // ***********
    // GetSchedulesForDateAsync Method
    // ***********

    // This method retrieves a list of daily schedules that are associated with tasks due on a specific date.
    // It includes related task data using the Include method for eager loading.
    public async Task<List<DailySchedule>> GetSchedulesForDateAsync(DateTime date)
    {
        // Query the database for daily schedules where the associated task's DueDate matches the provided date.
        // The Include method ensures that related Task data is loaded with each DailySchedule.
        return await _context.DailySchedules
            .Include(ds => ds.Task) // Include related Task objects for each schedule.
            .Where(ds => ds.Task.DueDate.HasValue && ds.Task.DueDate.Value.Date == date.Date) // Filter by task's DueDate.
            .ToListAsync();
    }







    // End of Controller 

    }
}
