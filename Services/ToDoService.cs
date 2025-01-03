using BudgetTracker.Data;
using BudgetTracker.Interfaces;
using BudgetTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Services
{
    public class ToDoService : IToDoService
    {
        private readonly ApplicationDbContext _context;

        public ToDoService(ApplicationDbContext context)
        {
            _context = context;
        }

public async Task<List<ToDoItem>> GetTodayTasksAsync()
{
    var today = DateTime.Today;
    return await _context.ToDoItems
        .Where(t =>
            // tasks with an actual due date of today
            (t.DueDate.HasValue && t.DueDate.Value.Date == today)
            // OR tasks flagged as "IsToday" or "IsTodayOnly"
            || t.IsTodayOnly
            || t.IsToday
        )
        .ToListAsync();
}



    public async Task AssignTaskToTimeAsync(int taskId, int hour, DateTime date, int minute)
    {
        // Your existing logic goes here, e.g. create a new DailySchedule:
        var schedule = new DailySchedule
        {
            TaskId = taskId,
            Hour = hour,
            Minute = minute, // <-- Using the new parameter
            Date = date.Date
        };

        _context.DailySchedules.Add(schedule);
        await _context.SaveChangesAsync();
    }



        public async Task<List<ToDoItem>> GetDailyTasksAsync()
        {
            return await _context.ToDoItems
                .Where(t => t.IsDaily)
                .ToListAsync();
        }

public async Task<List<DailySchedule>> GetDailySchedulesAsync()
{
    var today = DateTime.Today;

    return await _context.DailySchedules
        .Include(ds => ds.Task)
        .Where(ds => ds.Date.Date == today) // Only include schedules for today
        .ToListAsync();
}


    public async Task<List<DailySchedule>> GetAllSchedulesAsync()
    {
        return await _context.DailySchedules
            .Include(ds => ds.Task)
            .ToListAsync();
    }

public async Task CreateTaskAsync(ToDoItem task)
{
    Console.WriteLine($"Saving Task: Name={task.Name}, DueDate={task.DueDate}, IsDaily={task.IsDaily}, IsTodayOnly={task.IsTodayOnly}, IsToday={task.IsToday}");

    _context.ToDoItems.Add(task);
    await _context.SaveChangesAsync();
}





        public async Task MarkTaskAsCompleteAsync(int id)
        {
            var task = await _context.ToDoItems.FindAsync(id);
            if (task != null)
            {
                task.IsCompleted = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteTaskAsync(int id)
        {
            var task = await _context.ToDoItems.FindAsync(id);
            if (task != null)
            {
                _context.ToDoItems.Remove(task);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AssignTaskToTimeAsync(int taskId, int hour, DateTime date)
        {
            var task = await _context.ToDoItems.FindAsync(taskId);
            if (task == null)
                throw new Exception("Task not found.");

            var schedule = new DailySchedule
            {
                TaskId = taskId,
                Hour = hour,
                Date = date.Date
            };

            _context.DailySchedules.Add(schedule);
            await _context.SaveChangesAsync();
        }




public async Task<List<ToDoItem>> GetAllTasksAsync()
{
    return await _context.ToDoItems
        .Where(t => !t.IsTodayOnly) // Exclude "Today Only" tasks
        .ToListAsync();
}

public async Task UnassignTaskAsync(int taskId, int hour)
{
    var schedule = await _context.DailySchedules
        .FirstOrDefaultAsync(ds => ds.TaskId == taskId && ds.Hour == hour);

    if (schedule != null)
    {
        _context.DailySchedules.Remove(schedule); // Remove the assignment
        await _context.SaveChangesAsync();
    }
    else
    {
        throw new InvalidOperationException("Task not found or not assigned.");
    }
}
public async Task MoveTaskToTodayAsync(int taskId)
{
    Console.WriteLine($"MoveTaskToTodayAsync invoked with TaskId: {taskId}");
    var task = await _context.ToDoItems.FindAsync(taskId);

    if (task != null && (!task.DueDate.HasValue || task.DueDate.Value.Date >= DateTime.Today))
    {
        task.IsToday = true;
        await _context.SaveChangesAsync();
        Console.WriteLine($"Task {taskId} moved to today's tasks.");
    }
    else
    {
        Console.WriteLine($"Task {taskId} not found or not valid for today's tasks.");
    }
}


public async Task<List<ToDoItem>> GetTasksForDateAsync(DateTime selectedDate)
{
    return await _context.ToDoItems
        .Where(t => t.DueDate.HasValue && t.DueDate.Value.Date >= selectedDate.Date)
        .ToListAsync();
}


public async Task<List<DailySchedule>> GetSchedulesForDateAsync(DateTime date)
{
    return await _context.DailySchedules
        .Include(ds => ds.Task)
        .Where(ds => ds.Date.Date == date.Date)
        .ToListAsync();
}



    }
}
