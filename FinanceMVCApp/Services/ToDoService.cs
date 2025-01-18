using BudgetTracker.Data;
using BudgetTracker.Interfaces;
using BudgetTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Services
{
    /// <summary>
    /// Service class responsible for managing To-Do items within the BudgetTracker application.
    /// </summary>
    /// <remarks>
    /// This class provides methods for creating, retrieving, updating, and deleting to-do items,
    /// as well as performing various business logic operations related to task management.
    /// </remarks>
    public class ToDoService : IToDoService
    {
        // Dependency: Application database context for accessing and managing to-do item data
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToDoService"/> class.
        /// </summary>
        /// <param name="context">The database context to be used for accessing the application's data.</param>
        public ToDoService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }





    //**************
    // GET TODAYS TASKS
    //**************

    /// <summary>
    /// Retrieves a list of to-do items that are relevant for today.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a list of 
    /// <see cref="ToDoItem"/> objects that are due today or flagged as "IsToday" or "IsTodayOnly."
    /// </returns>
    /// <remarks>
    /// This method filters tasks based on the following criteria:
    /// <list type="bullet">
    /// <item>
    /// Tasks with a <see cref="ToDoItem.DueDate"/> that matches today's date.
    /// </item>
    /// <item>
    /// Tasks marked as <see cref="ToDoItem.IsTodayOnly"/>.
    /// </item>
    /// <item>
    /// Tasks marked as <see cref="ToDoItem.IsToday"/>.
    /// </item>
    /// </list>
    /// </remarks>
    public async Task<List<ToDoItem>> GetTodayTasksAsync()
    {
        // Define today's date as a baseline for filtering
        var today = DateTime.Today;

        // Query the database to find all tasks matching the criteria for today's tasks
        return await _context.ToDoItems
            .Where(t =>
                // Include tasks with a DueDate matching today's date
                (t.DueDate.HasValue && t.DueDate.Value.Date == today)
                // OR tasks explicitly flagged as "IsTodayOnly"
                || t.IsTodayOnly
                // OR tasks flagged as "IsToday" (repeated daily tasks)
                || t.IsToday
            )
            .ToListAsync(); // Execute the query asynchronously and return the results as a list
    }



    //**************
    // ASSIGN A TASK TO A TIME
    //**************

    /// <summary>
    /// Assigns a specific task to a designated time slot on a given date.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task to be assigned.</param>
    /// <param name="hour">The hour (24-hour format) at which the task should be scheduled.</param>
    /// <param name="date">The date to which the task should be assigned.</param>
    /// <param name="minute">The minute within the hour at which the task should be scheduled (e.g., 0 or 30).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method creates a new entry in the `DailySchedules` table with the specified
    /// task, time, and date. It ensures the task is scheduled for a precise time slot
    /// and commits the change to the database.
    /// </remarks>
    public async Task AssignTaskToTimeAsync(int taskId, int hour, DateTime date, int minute)
    {
        // Validate the input parameters
        // Ensure the provided hour is within valid bounds (0-23)
        if (hour < 0 || hour > 23)
        {
            throw new ArgumentOutOfRangeException(nameof(hour), "Hour must be between 0 and 23.");
        }

        // Ensure the minute is valid (0-59)
        if (minute < 0 || minute > 59)
        {
            throw new ArgumentOutOfRangeException(nameof(minute), "Minute must be between 0 and 59.");
        }

        // Ensure the taskId exists in the database
        var taskExists = await _context.Tasks.AnyAsync(t => t.Id == taskId);
        if (!taskExists)
        {
            throw new InvalidOperationException("Task with the specified ID does not exist.");
        }

        // Create a new DailySchedule entry with the provided details
        var schedule = new DailySchedule
        {
            TaskId = taskId,       // Associate the schedule with the specified task
            Hour = hour,           // Hour at which the task is scheduled
            Minute = minute,       // Minute within the hour
            Date = date.Date       // Ensure only the date part is used
        };

        // Add the new schedule to the database context
        _context.DailySchedules.Add(schedule);

        // Save the changes asynchronously to persist the schedule in the database
        await _context.SaveChangesAsync();
    }




    //**************
    // GET DAILY TASKS
    //**************

    /// <summary>
    /// Retrieves all tasks that are marked as daily tasks.
    /// </summary>
    /// <returns>
    /// A list of <see cref="ToDoItem"/> objects that have the <c>IsDaily</c> flag set to <c>true</c>.
    /// </returns>
    /// <remarks>
    /// This method queries the `ToDoItems` table in the database and fetches all tasks
    /// where the `IsDaily` property is set to <c>true</c>. These tasks are considered
    /// to be recurring and are expected to be shown or executed daily.
    /// </remarks>
    public async Task<List<ToDoItem>> GetDailyTasksAsync()
    {
        // Query the database to retrieve tasks marked as daily
        // The `IsDaily` property is used to filter tasks intended to recur every day
        return await _context.ToDoItems
            .Where(t => t.IsDaily) // Filter only tasks with IsDaily == true
            .ToListAsync(); // Execute the query and return the results as a list
    }







    //**************
    // GET DAILY SCHEDULE
    //**************

    /// <summary>
    /// Retrieves all daily schedules for the current day.
    /// </summary>
    /// <returns>
    /// A list of <see cref="DailySchedule"/> objects for the current date.
    /// Each schedule includes the associated task details through eager loading.
    /// </returns>
    /// <remarks>
    /// This method queries the `DailySchedules` table in the database and filters
    /// schedules to include only those with a `Date` matching today's date.
    /// The associated tasks are included using eager loading to ensure all necessary
    /// details are available without requiring additional queries.
    /// </remarks>
    public async Task<List<DailySchedule>> GetDailySchedulesAsync()
    {
        // Get today's date (midnight of the current day)
        var today = DateTime.Today;

        // Query the database for daily schedules with the current date
        // Use eager loading to include the associated Task details
        return await _context.DailySchedules
            .Include(ds => ds.Task) // Eagerly load related Task data
            .Where(ds => ds.Date.Date == today) // Filter schedules for today only
            .ToListAsync(); // Execute the query asynchronously and return the results as a list
    }






    //**************
    // GET ALL SCHEDULES
    //**************

    /// <summary>
    /// Retrieves all daily schedules from the database, including their associated tasks.
    /// </summary>
    /// <returns>
    /// A list of <see cref="DailySchedule"/> objects representing all schedules in the system.
    /// Each schedule includes its associated task details through eager loading.
    /// </returns>
    /// <remarks>
    /// This method queries the `DailySchedules` table to fetch all records, regardless of the date.
    /// Associated tasks are loaded using eager loading to ensure task details are readily available.
    /// </remarks>
    public async Task<List<DailySchedule>> GetAllSchedulesAsync()
    {
        // Query the database for all daily schedules
        // Use eager loading to include the associated Task details
        return await _context.DailySchedules
            .Include(ds => ds.Task) // Eagerly load related Task data
            .ToListAsync(); // Execute the query asynchronously and return the results as a list
    }









    //**************
    // CREATE A NEW TASK
    //**************

    /// <summary>
    /// Adds a new to-do task to the database.
    /// </summary>
    /// <param name="task">
    /// An instance of <see cref="ToDoItem"/> representing the task to be created.
    /// This task should include all required fields such as Name, DueDate, and relevant flags (e.g., IsDaily, IsTodayOnly, IsToday).
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous operation. The task's state is saved in the database.
    /// </returns>
    /// <remarks>
    /// - This method ensures the provided task is added to the database and persists the changes asynchronously.
    /// - Includes debug logging to provide visibility into the details of the task being saved.
    /// </remarks>
    public async Task CreateTaskAsync(ToDoItem task)
    {
        // Log the task details to the console for debugging purposes
        // This helps track the task attributes when the method is invoked
        Console.WriteLine($"Saving Task: Name={task.Name}, DueDate={task.DueDate}, IsDaily={task.IsDaily}, IsTodayOnly={task.IsTodayOnly}, IsToday={task.IsToday}");

        // Add the task entity to the database context for tracking
        _context.ToDoItems.Add(task);

        // Persist the changes to the database asynchronously
        await _context.SaveChangesAsync();
    }









    //**************
    // MARK A TASK COMPLETED
    //**************

    /// <summary>
    /// Marks a to-do task as complete in the database.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the task to mark as completed.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous operation.
    /// </returns>
    /// <remarks>
    /// - This method updates the `IsCompleted` property of a task to `true`.
    /// - If the task with the specified ID does not exist, the method completes without making changes.
    /// - The operation is performed asynchronously to ensure efficient database interactions.
    /// </remarks>
    public async Task MarkTaskAsCompleteAsync(int id)
    {
        // Attempt to retrieve the task from the database by its unique ID
        var task = await _context.ToDoItems.FindAsync(id);

        // Check if the task exists
        if (task != null)
        {
            // Mark the task as completed
            task.IsCompleted = true;

            // Save the changes asynchronously to the database
            await _context.SaveChangesAsync();
        }
        // If the task does not exist, the method does nothing
    }








    //**************
    // DELETE A TASK
    //**************

    /// <summary>
    /// Deletes a to-do task from the database if it exists.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the task to delete.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous operation.
    /// </returns>
    /// <remarks>
    /// - This method retrieves the task by its ID and removes it from the database.
    /// - If the task with the specified ID does not exist, the method completes without making changes.
    /// - The operation is performed asynchronously to ensure efficient database interactions.
    /// </remarks>
    public async Task DeleteTaskAsync(int id)
    {
        // Attempt to find the task in the database using its unique ID
        var task = await _context.ToDoItems.FindAsync(id);

        // Check if the task exists in the database
        if (task != null)
        {
            // Remove the task from the database context
            _context.ToDoItems.Remove(task);

            // Save the changes to persist the deletion
            await _context.SaveChangesAsync();
        }
        // If the task does not exist, the method exits without performing any operation
    }





    //**************
    // ASSIGN A TASK TO A TIME
    //**************

    /// <summary>
    /// Assigns a to-do task to a specific time and date.
    /// </summary>
    /// <param name="taskId">
    /// The unique identifier of the task to be scheduled.
    /// </param>
    /// <param name="hour">
    /// The hour of the day (in 24-hour format) at which the task should be scheduled.
    /// </param>
    /// <param name="date">
    /// The date on which the task should be scheduled.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous operation.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown if the task with the specified <paramref name="taskId"/> cannot be found in the database.
    /// </exception>
    /// <remarks>
    /// - This method first validates whether the task exists in the database.
    /// - If the task exists, it creates a new entry in the `DailySchedules` table linking the task to the specified time and date.
    /// - The operation is performed asynchronously to ensure scalability and non-blocking database interactions.
    /// </remarks>
    public async Task AssignTaskToTimeAsync(int taskId, int hour, DateTime date)
    {
        // Step 1: Retrieve the task by its ID from the database.
        var task = await _context.ToDoItems.FindAsync(taskId);

        // Step 2: Validate that the task exists. If not, throw an exception.
        if (task == null)
            throw new Exception("Task not found."); // Exception is thrown for invalid task IDs.

        // Step 3: Create a new DailySchedule object linking the task to the specified time and date.
        var schedule = new DailySchedule
        {
            TaskId = taskId,  // Link to the task by ID.
            Hour = hour,      // Assign the specified hour (in 24-hour format).
            Date = date.Date  // Use only the date part, ignoring the time component.
        };

        // Step 4: Add the new schedule to the DailySchedules table in the database.
        _context.DailySchedules.Add(schedule);

        // Step 5: Save the changes to the database asynchronously.
        await _context.SaveChangesAsync();
    }




    //**************
    // GET ALL TASKS
    //**************

    /// <summary>
    /// Retrieves all to-do tasks from the database, excluding tasks that are flagged as "Today Only."
    /// </summary>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation, containing a list of all <see cref="ToDoItem"/> objects that are not flagged as "Today Only."
    /// </returns>
    /// <remarks>
    /// - This method fetches all tasks from the `ToDoItems` table in the database, filtering out tasks marked as "Today Only."
    /// - Tasks marked as "Today Only" are excluded because they are intended to be temporary and only relevant for the current day.
    /// - The operation is performed asynchronously to ensure non-blocking behavior for scalable applications.
    /// </remarks>
    public async Task<List<ToDoItem>> GetAllTasksAsync()
    {
        // Step 1: Query the database for all tasks that are not flagged as "Today Only."
        // This ensures only persistent tasks are included in the result set.
        return await _context.ToDoItems
            .Where(t => !t.IsTodayOnly) // Exclude tasks that are specifically marked for "Today Only."
            .ToListAsync(); // Asynchronously execute the query and convert the result to a list.
    }









    //**************
    // UNASSIGN A TASK
    //**************

    /// <summary>
    /// Unassigns a task from a specific hour by removing the corresponding entry
    /// in the <see cref="DailySchedule"/> table in the database.
    /// </summary>
    /// <param name="taskId">
    /// The unique identifier of the task to unassign.
    /// </param>
    /// <param name="hour">
    /// The hour associated with the task assignment to be removed.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation. If no matching assignment is found,
    /// an <see cref="InvalidOperationException"/> is thrown.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no matching <see cref="DailySchedule"/> is found for the provided task ID and hour.
    /// </exception>
    public async Task UnassignTaskAsync(int taskId, int hour)
    {
        // Attempt to retrieve the matching DailySchedule entity for the specified task and hour.
        var schedule = await _context.DailySchedules
            .FirstOrDefaultAsync(ds => ds.TaskId == taskId && ds.Hour == hour);

        if (schedule != null)
        {
            // Remove the schedule entry if found.
            _context.DailySchedules.Remove(schedule);
            
            // Save changes to the database to persist the removal.
            await _context.SaveChangesAsync();
        }
        else
        {
            // If no matching entity is found, throw an exception with a descriptive error message.
            throw new InvalidOperationException("Task not found or not assigned.");
        }
    }







    //**************
    // MOVE A TASK TO TODAY
    //**************

    /// <summary>
    /// Moves a specified task to today's tasks by updating its <see cref="IsToday"/> flag.
    /// </summary>
    /// <param name="taskId">
    /// The unique identifier of the task to be moved to today's tasks.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation. The task modifies
    /// the specified task in the database, ensuring it is marked for today's tasks.
    /// </returns>
    /// <remarks>
    /// This method modifies the <see cref="IsToday"/> property of the task if the task meets the following criteria:
    /// - The task exists in the database.
    /// - The task's due date is either unset (null) or a future date (including today).
    /// If these conditions are not met, the task will not be updated, and a message will be logged to the console.
    /// </remarks>
    public async Task MoveTaskToTodayAsync(int taskId)
    {
        // Log the invocation of the method for debugging or tracking purposes.
        Console.WriteLine($"MoveTaskToTodayAsync invoked with TaskId: {taskId}");

        // Attempt to retrieve the task with the specified ID from the database.
        var task = await _context.ToDoItems.FindAsync(taskId);

        // Check if the task exists and meets the criteria for being moved to today's tasks.
        if (task != null && (!task.DueDate.HasValue || task.DueDate.Value.Date >= DateTime.Today))
        {
            // Mark the task as part of today's tasks.
            task.IsToday = true;

            // Persist the changes to the database.
            await _context.SaveChangesAsync();

            // Log a success message indicating that the task has been moved.
            Console.WriteLine($"Task {taskId} moved to today's tasks.");
        }
        else
        {
            // Log a message indicating that the task was not found or is not valid for today's tasks.
            Console.WriteLine($"Task {taskId} not found or not valid for today's tasks.");
        }
    }




    //**************
    // GET TASKS FOR A DATE
    //**************

    /// <summary>
    /// Retrieves a list of to-do items with a due date on or after the specified date.
    /// </summary>
    /// <param name="selectedDate">
    /// The date for which tasks are to be retrieved. Tasks with a due date equal to or later
    /// than this date will be included in the result.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation, containing a <see cref="List{ToDoItem}"/>
    /// of to-do items that match the criteria.
    /// </returns>
    /// <remarks>
    /// This method filters tasks stored in the database based on their due date. Only tasks
    /// with a non-null due date that is equal to or later than the specified <paramref name="selectedDate"/>
    /// will be included in the result.
    /// </remarks>
    public async Task<List<ToDoItem>> GetTasksForDateAsync(DateTime selectedDate)
    {
        // Asynchronously query the database to retrieve tasks with a due date matching the criteria.
        return await _context.ToDoItems
            .Where(t => 
                t.DueDate.HasValue &&             // Ensure the task has a due date set.
                t.DueDate.Value.Date >= selectedDate.Date // Compare due date to the selected date.
            )
            .ToListAsync(); // Convert the query results to a list.
    }









    //**************
    // GET SCHEDULE FOR A DATE
    //**************

    /// <summary>
    /// Retrieves the daily schedules for a specific date.
    /// </summary>
    /// <param name="date">
    /// The date for which the schedules are to be retrieved.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation, containing a <see cref="List{DailySchedule}"/>
    /// of daily schedules for the specified date, including their associated tasks.
    /// </returns>
    /// <remarks>
    /// This method queries the database to fetch all daily schedules where the <see cref="DailySchedule.Date"/>
    /// matches the provided <paramref name="date"/>. Each result includes the associated task details,
    /// leveraging Entity Framework's eager loading.
    /// </remarks>
    public async Task<List<DailySchedule>> GetSchedulesForDateAsync(DateTime date)
    {
        // Query the database to retrieve all daily schedules for the given date.
        // The query includes associated Task entities for detailed schedule information.
        return await _context.DailySchedules
            .Include(ds => ds.Task)           // Include the Task entity associated with each schedule
            .Where(ds => ds.Date.Date == date.Date) // Filter by the exact date, ignoring time
            .ToListAsync();                   // Execute the query asynchronously and return the results as a list
    }




























    // End of ToDo Services 

    }
}
