using BudgetTracker.Data;
using BudgetTracker.Interfaces;
using BudgetTracker.Models;
using BudgetTracker.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Services
{
    /// <summary>
    /// Provides services for managing and handling schedules in the application.
    /// </summary>
    public class ScheduleService : IScheduleService
    {
        // The database context used to interact with the application's data store.
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleService"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        /// <remarks>
        /// This constructor is responsible for injecting the database context, which
        /// allows the service to interact with the underlying database for CRUD operations.
        /// </remarks>
        public ScheduleService(ApplicationDbContext context)
        {
            // Assign the injected database context to the local field for use in methods.
            _context = context;
        }







    //**************
    // GET SCHEDULE 
    //**************

    /// <summary>
    /// Retrieves a schedule containing tasks grouped into three categories: 
    /// - Current week's tasks
    /// - Upcoming week's tasks
    /// - Tasks beyond the next two weeks
    /// </summary>
    /// <returns>
    /// A <see cref="ScheduleViewModel"/> object containing grouped tasks.
    /// </returns>
    /// <remarks>
    /// This method organizes tasks based on their start and end dates relative to the current date.
    /// It fetches all tasks from the database, removes duplicates, and categorizes them.
    /// </remarks>
    public async Task<ScheduleViewModel> GetScheduleAsync()
    {
        // Get the current date and time
        var currentDate = DateTime.Now;

        // Fetch all tasks from the database asynchronously
        var allTasks = await _context.Tasks.ToListAsync();

        // Remove duplicate tasks by grouping them based on their unique ID
        // Only the first instance of each task ID is retained
        allTasks = allTasks.GroupBy(t => t.Id)
                        .Select(g => g.First())
                        .ToList();

        // Define the start and end of the current week
        var startOfWeek = currentDate.StartOfWeek(); // Custom extension method for determining week's start
        var endOfWeek = currentDate.EndOfWeek(); // Custom extension method for determining week's end

        // Filter tasks that fall within the current week
        // Include tasks that start before the end of the week and end after the start of the week
        var currentWeekTasks = allTasks
            .Where(t => t.StartDate <= endOfWeek && t.EndDate >= startOfWeek)
            .ToList();

        // Filter tasks that fall within the upcoming week (7 days after the current week's end)
        var upcomingWeekTasks = allTasks
            .Where(t => t.StartDate > endOfWeek && t.StartDate <= endOfWeek.AddDays(7))
            .ToList();

        // Filter tasks that fall beyond the upcoming two weeks
        var farthestTasks = allTasks
            .Where(t => t.StartDate > endOfWeek.AddDays(7))
            .ToList();

        // Return the tasks grouped into the current week, upcoming week, and future categories
        return new ScheduleViewModel
        {
            CurrentWeekTasks = currentWeekTasks, // Tasks in the current week
            UpcomingWeekTasks = upcomingWeekTasks, // Tasks in the next week
            FarthestTasks = farthestTasks // Tasks beyond the next two weeks
        };
    }






    //**************
    // ADD A MULTI DAY TASK
    //**************

    // Multi-day task
    /// <summary>
    /// Adds a new multi-day task to the database.
    /// </summary>
    /// <param name="name">The name of the task.</param>
    /// <param name="startDate">The start date of the task.</param>
    /// <param name="endDate">The end date of the task.</param>
    /// <param name="time">The time of the task.</param>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="startDate"/> is later than <paramref name="endDate"/>.</exception>
    /// <remarks>
    /// This method is used to create tasks spanning multiple days. It ensures that the task's start date is earlier than or equal to its end date.
    /// </remarks>
    public async Task AddTaskAsync(string name, DateTime startDate, DateTime endDate, TimeSpan time)
    {
        // Validate that the start date is not after the end date
        if (startDate > endDate)
            throw new ArgumentException("StartDate cannot be later than EndDate.");

        // Create a new task object with the specified date range and time
        var newTask = new TaskItem
        {
            Name = name,
            StartDate = startDate,
            EndDate = endDate,
            Time = time
        };

        // Add the task to the database context
        _context.Tasks.Add(newTask);

        // Save the changes to the database
        await _context.SaveChangesAsync();
    }






    //**************
    // ADD A SINGLE DAY TAASK WITH DATE AND TIME
    //**************

    // Single-day task with date and time
    /// <summary>
    /// Adds a new single-day task to the database, specifying both the date and time.
    /// </summary>
    /// <param name="name">The name of the task.</param>
    /// <param name="date">The date of the task.</param>
    /// <param name="time">The time of the task.</param>
    /// <remarks>
    /// This method is used for tasks that occur on a single day at a specific time.
    /// The <c>StartDate</c> and <c>EndDate</c> are set to the same value.
    /// </remarks>
    public async Task AddTaskAsync(string name, DateTime date, TimeSpan time)
    {
        // Create a new task object with the specified date and time
        var newTask = new TaskItem
        {
            Name = name,
            StartDate = date,
            EndDate = date, // Single-day task has the same start and end date
            Time = time
        };

        // Add the task to the database context
        _context.Tasks.Add(newTask);

        // Save the changes to the database
        await _context.SaveChangesAsync();
    }






    //**************
    // ADD A TASK WITH JUST A DATE 
    //**************

    // Single-day task with only date
    /// <summary>
    /// Adds a new single-day task to the database, specifying only the date.
    /// </summary>
    /// <param name="name">The name of the task.</param>
    /// <param name="date">The date of the task.</param>
    /// <remarks>
    /// This method is used for tasks that occur on a specific day but do not have a specific time.
    /// The <c>StartDate</c> and <c>EndDate</c> are set to the same value.
    /// </remarks>
    public async Task AddTaskAsync(string name, DateTime date)
    {
        // Create a new task object with the specified date
        var newTask = new TaskItem
        {
            Name = name,
            StartDate = date,
            EndDate = date // Single-day task has the same start and end date
        };

        // Add the task to the database context
        _context.Tasks.Add(newTask);

        // Save the changes to the database
        await _context.SaveChangesAsync();
    }






    //**************
    // GET TASKS FOR THE CURRENT WEEK
    //**************

    /// <summary>
    /// Retrieves a list of tasks scheduled for the current week.
    /// </summary>
    /// <returns>A list of <see cref="TaskItem"/> objects that fall within the current week.</returns>
    /// <remarks>
    /// The method calculates the start and end dates of the current week based on the current date.
    /// It then queries the database for tasks where the task's start or end date overlaps with this range.
    /// </remarks>
    public async Task<List<TaskItem>> GetTasksForCurrentWeekAsync()
    {
        // Get the current date and time
        var currentDate = DateTime.Now;

        // Calculate the start of the current week (e.g., Sunday)
        var startOfWeek = currentDate.StartOfWeek();

        // Calculate the end of the current week (e.g., Saturday)
        var endOfWeek = currentDate.EndOfWeek();

        // Query the database for tasks that overlap with the current week
        // A task is included if:
        // - Its StartDate is before or on the end of the week, AND
        // - Its EndDate is after or on the start of the week
        return await _context.Tasks
            .Where(t => t.StartDate <= endOfWeek && t.EndDate >= startOfWeek)
            .ToListAsync();
    }






    //**************
    // DELETE A TASK
    //**************

    /// <summary>
    /// Deletes a task from the database by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the task to delete.</param>
    /// <returns>
    /// A boolean value indicating whether the deletion was successful:
    /// - <c>true</c>: The task was found and successfully deleted.
    /// - <c>false</c>: The task with the specified ID was not found.
    /// </returns>
    /// <remarks>
    /// This method retrieves the task by its ID and, if found, removes it from the database context.
    /// The changes are saved to persist the deletion.
    /// </remarks>
    public async Task<bool> DeleteTaskAsync(int id)
    {
        // Find the task in the database using its ID
        var task = await _context.Tasks.FindAsync(id);

        // If no task is found, return false to indicate failure
        if (task == null)
        {
            return false; // Task not found
        }

        // Mark the task for deletion
        _context.Tasks.Remove(task);

        // Persist the deletion in the database
        await _context.SaveChangesAsync();

        // Return true to indicate the task was successfully deleted
        return true;
    }








































    // End of Schedule Services 
    }
}
