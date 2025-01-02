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
            return await _context.ToDoItems
                .Where(t => t.IsToday)
                .ToListAsync(); // Ensure it includes tasks with IsToday = true
        }



        public async Task<List<ToDoItem>> GetDailyTasksAsync()
        {
            return await _context.ToDoItems
                .Where(t => t.IsDaily)
                .ToListAsync();
        }

        public async Task<List<DailySchedule>> GetDailySchedulesAsync()
        {
            return await _context.DailySchedules
                .Include(ds => ds.Task)
                .ToListAsync();
                }
        public async Task CreateTaskAsync(ToDoItem task)
        {
            Console.WriteLine($"Saving Task: Name={task.Name}, DueDate={task.DueDate}, IsDaily={task.IsDaily}, IsTodayOnly={task.IsTodayOnly}");

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

public async Task AssignTaskToTimeAsync(int taskId, int hour)
{
    Console.WriteLine($"AssignTaskToTimeAsync called with TaskId: {taskId}, Hour: {hour}");

    var task = await _context.ToDoItems.FindAsync(taskId);
    if (task == null)
    {
        Console.WriteLine($"TaskId: {taskId} not found.");
        throw new Exception("Task not found.");
    }

    var schedule = new DailySchedule
    {
        TaskId = taskId,
        Hour = hour
    };

    Console.WriteLine($"Adding task to schedule: TaskId: {schedule.TaskId}, Hour: {schedule.Hour}");
    _context.DailySchedules.Add(schedule);

    await _context.SaveChangesAsync();
    Console.WriteLine($"TaskId: {taskId} successfully assigned to Hour: {hour}");
}


        public async Task<List<ToDoItem>> GetAllTasksAsync()
        {
            return await _context.ToDoItems.ToListAsync(); // Fetch all tasks from the database
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
    var task = await _context.ToDoItems.FindAsync(taskId); // Correct entity
    if (task != null)
    {
        task.IsToday = true;
        await _context.SaveChangesAsync();
        Console.WriteLine($"Task {taskId} moved to today's tasks.");
    }
    else
    {
        Console.WriteLine($"Task {taskId} not found.");
    }
}

        public async Task<List<ToDoItem>> GetTasksForDateAsync(DateTime date)
        {
            return await _context.ToDoItems
                .Where(t => t.DueDate.Date == date.Date)
                .ToListAsync();
        }

        public async Task<List<DailySchedule>> GetSchedulesForDateAsync(DateTime date)
        {
            return await _context.DailySchedules
                .Include(ds => ds.Task)
                .Where(ds => ds.Task.DueDate.Date == date.Date)
                .ToListAsync();
        }

    }
}
