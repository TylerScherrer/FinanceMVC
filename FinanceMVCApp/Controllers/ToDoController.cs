using BudgetTracker.Interfaces;
using BudgetTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Controllers
{
    // ***********
    // ToDoController Class
    // ***********

    // The ToDoController class manages actions related to to-do items and schedules.
    // It utilizes the IToDoService interface to interact with the application's to-do logic and data.

    public class ToDoController : Controller
    {
        // ***********
        // Dependency Fields
        // ***********

        // Private readonly field to hold a reference to the IToDoService.
        // This service provides the business logic for managing to-do items and schedules.
        private readonly IToDoService _toDoService;

        // ***********
        // Constructor
        // ***********

        // The constructor initializes the ToDoController with the required IToDoService dependency.
        // Dependency Injection (DI) ensures that the controller is provided with an instance of IToDoService.
        // This promotes loose coupling and simplifies unit testing.
        public ToDoController(IToDoService toDoService)
        {
            // Assign the provided IToDoService instance to the private _toDoService field.
            // This allows the controller to call methods on the service to manage to-do items and schedules.
            _toDoService = toDoService;
        }






    // ***********
    // GET: All Tasks
    // ***********

    // This method handles the GET request for the Index action of the ToDoController.
    // It retrieves and displays tasks for a specific date (defaulting to today's date) and all tasks available in the system.
    public async Task<IActionResult> Index(DateTime? date)
    {
        // Determine the selected date. If no date is provided, use today's date as the default.
        var selectedDate = date ?? DateTime.Today;

        // Fetch tasks specifically for today using the ToDoService.
        var todayTasks = await _toDoService.GetTodayTasksAsync();

        // Fetch all tasks using the ToDoService.
        var allTasks = await _toDoService.GetAllTasksAsync();

        // Create a ViewModel to organize data for rendering in the view.
        // The ViewModel combines selected date, today's tasks, all tasks, and an optional empty daily schedule list.
        var viewModel = new BudgetWithTasksViewModel
        {
            SelectedDate = selectedDate,              // The date currently selected by the user.
            TodayTasks = todayTasks,                  // A list of tasks specifically for today.
            AllTasks = allTasks,                      // A list of all tasks in the system.
            DailySchedules = new List<DailySchedule>() // Empty list placeholder for schedules, can be populated if needed later.
        };

        // Pass the ViewModel to the Index view for rendering.
        return View(viewModel);
    }





    // ***********
    // GET: Daily Tasks
    // ***********

    // This method handles the GET request for the DailyList action of the ToDoController.
    // It retrieves and displays the list of daily tasks.
    public async Task<IActionResult> DailyList()
    {
        // Fetch the list of daily tasks using the ToDoService.
        var dailyTasks = await _toDoService.GetDailyTasksAsync();

        // Pass the dailyTasks to the DailyList view for rendering.
        return View(dailyTasks);
    }





    // ***********
    // GET: Add New Task
    // ***********

    // This method handles the GET request for the Create action of the ToDoController.
    // It prepares the form for creating a new task.
    public IActionResult Create()
    {
        // Render the Create view, which contains a form for adding a new task.
        return View();
    }






    // ***********
    // POST: Create a New Task
    // ***********

    // This method handles the POST request for creating a new task.
    // It validates the task data, processes specific conditions (e.g., tasks for today only), 
    // and then saves the task using the ToDoService.
    [HttpPost]
    public async Task<IActionResult> Create(ToDoItem task)
    {
        // Log the task details for debugging purposes.
        Console.WriteLine($"Name: {task.Name}, DueDate: {task.DueDate}, IsDaily: {task.IsDaily}, IsTodayOnly: {task.IsTodayOnly}");

        // Check if the model data is valid based on validation attributes defined in the ToDoItem model.
        if (ModelState.IsValid)
        {
            // Handle tasks marked as "today only".
            if (task.IsTodayOnly)
            {
                task.IsToday = true; // Mark the task as for today only.
                task.DueDate = null; // Remove the due date since it's specific to today.
            }
            else if (task.DueDate.HasValue)
            {
                // Mark the task as "for today" if the due date matches today's date.
                task.IsToday = task.DueDate.Value.Date == DateTime.Today;
            }

            // Save the task to the database using the service.
            await _toDoService.CreateTaskAsync(task);

            // Redirect to the Index action after successfully creating the task.
            return RedirectToAction(nameof(Index));
        }

        // If the model state is invalid, log the validation errors for debugging.
        foreach (var key in ModelState.Keys)
        {
            var state = ModelState[key];
            foreach (var error in state.Errors)
            {
                Console.WriteLine($"Error in {key}: {error.ErrorMessage}");
            }
        }

        // Return the view with the current task model to display validation errors and allow corrections.
        return View(task);
    }





    // ***********
    // POST: Mark Task as Complete
    // ***********

    // This method handles the POST request for marking a task as complete.
    // It updates the task's status to "completed" in the database using the ToDoService.
    [HttpPost]
    public async Task<IActionResult> MarkComplete(int id)
    {
        // Mark the task as complete by calling the service method with the task ID.
        await _toDoService.MarkTaskAsCompleteAsync(id);

        // Redirect the user back to the Index action to display the updated task list.
        return RedirectToAction(nameof(Index));
    }





    // ***********
    // POST: Delete a Task
    // ***********

    // This method handles the POST request for deleting a specific task.
    // It removes the task from the database using the ToDoService.
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        // Delete the task from the database by calling the service method with the task ID.
        await _toDoService.DeleteTaskAsync(id);

        // Redirect the user back to the Index action to display the updated task list.
        return RedirectToAction(nameof(Index));
    }


    // ***********
    // POST: Assign Task to a Specific Time
    // ***********

    // This method handles the assignment of a task (existing or newly created) to a specific time slot on a selected date.
    // It processes both user-input tasks (newTaskName) and existing tasks (taskId).
    // Asynchronous operation ensures responsiveness and scalability for large datasets.
    [HttpPost]
    public async Task<IActionResult> AssignTaskToTime(int? taskId, string newTaskName, int hour, int minute, DateTime selectedDate)
    {

        int newTaskId = 0; // Variable to store the ID of the newly created task.
        if (!string.IsNullOrWhiteSpace(newTaskName)) // Check if the user provided a new task name.
        {
            // Create a new ToDoItem instance with the provided name and date.
            var newTask = new ToDoItem
            {
                Name = newTaskName,       // Task name from user input.
                DueDate = selectedDate,   // Assign the task to the selected date.
                IsCompleted = false       // New tasks are incomplete by default.
            };

            // Save the new task to the database using the ToDoService.
            await _toDoService.CreateTaskAsync(newTask);
            newTaskId = newTask.Id; // Retrieve the ID of the newly created task.
        }

        // Use the new task ID if it was created; otherwise, fall back to the existing task ID.
        int finalTaskId = (newTaskId > 0) ? newTaskId : (taskId ?? 0);

        // Validate that a valid task ID is available.
        if (finalTaskId <= 0)
        {
            // Log an error and set an error message for the user.
            TempData["ErrorMessage"] = "Please select an existing task or enter a new task name.";
            return RedirectToAction("Index", "Planner", new { date = selectedDate });
        }

        try
        {
            // Assign the task to the specified time slot using the ToDoService.
            await _toDoService.AssignTaskToTimeAsync(finalTaskId, hour, selectedDate, minute);
        }
        catch (Exception ex)
        {
            // Handle any errors during assignment and log the exception.
            TempData["ErrorMessage"] = "Failed to assign task. " + ex.Message;
        }

        // Redirect the user back to the Planner view for the selected date.
        return RedirectToAction("Index", "Planner", new { date = selectedDate });
    }



        
    /// ***********
    /// Retrieve All Tasks
    /// ***********
    ///
    /// This method fetches a list of all tasks in the system asynchronously.
    /// It delegates the task retrieval responsibility to the ToDo service.
    /// 
    /// Returns:
    /// - A `Task` containing a list of `ToDoItem` objects retrieved by the ToDo service.
    public async Task<List<ToDoItem>> GetAllTasksAsync()
    {
        // Use the ToDoService to retrieve all tasks from the data source.
        return await _toDoService.GetAllTasksAsync();
    }





    /// ***********
    /// Unassign Task from a Specific Time
    /// ***********
    ///
    /// This POST method removes a task from a specific time slot in the daily schedule.
    /// 
    /// Parameters:
    /// - `taskId`: The ID of the task to be unassigned.
    /// - `hour`: The hour of the day (0-23) from which the task should be unassigned.
    /// 
    /// Returns:
    /// - Redirects to the Planner Index page.
    /// 
    /// Error Handling:
    /// - Validates the task ID and hour to ensure they are valid.
    /// - Logs and provides user feedback for any exceptions encountered.
    [HttpPost]
    public async Task<IActionResult> UnassignTask(int taskId, int hour)
    {
        // Validate the task ID and hour range.
        if (taskId <= 0 || hour < 0 || hour > 23)
        {
            ModelState.AddModelError("", "Invalid task or hour specified."); // Add validation error.
            return RedirectToAction("Index", "Planner"); // Redirect to Planner Index page.
        }

        try
        {
            // Unassign the task using the ToDo service.
            await _toDoService.UnassignTaskAsync(taskId, hour);

            // Optionally, store a success message in TempData for user feedback.
            TempData["SuccessMessage"] = "Task successfully unassigned.";
        }
        catch (Exception ex)
        {
            // Log the exception and provide error feedback to the user.
            Console.WriteLine($"Error unassigning task: {ex.Message}");
            TempData["ErrorMessage"] = "An error occurred while unassigning the task.";
        }

        // Redirect back to the Planner Index page.
        return RedirectToAction("Index", "Planner");
    }


    /// ***********
    /// Move Task to Today
    /// ***********
    ///
    /// This POST method moves a specified task to today's schedule.
    /// 
    /// Parameters:
    /// - `taskId`: The ID of the task to be moved.
    /// 
    /// Returns:
    /// - Redirects to the Index page.
    /// 
    /// Error Handling:
    /// - Logs and handles any errors encountered during the task update.
    [HttpPost]
    public async Task<IActionResult> MoveToToday(int taskId)
    {
        try
        {
            // Use the ToDo service to mark the task as belonging to today's schedule.
            await _toDoService.MoveTaskToTodayAsync(taskId);

            // Redirect to the Index page upon successful operation.
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            // Log the exception and add an error message to the ModelState.
            Console.WriteLine($"Error moving task: {ex.Message}");
            ModelState.AddModelError("", ex.Message);

            // Redirect back to the Index page with the error handled.
            return RedirectToAction("Index");
        }
    }







    // End of Controller 
    }
}
