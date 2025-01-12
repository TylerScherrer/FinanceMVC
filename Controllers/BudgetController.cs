using BudgetTracker.Interfaces;
using BudgetTracker.Models;
using BudgetTracker.Services;
using Microsoft.AspNetCore.Mvc;


namespace BudgetTracker.Controllers
{
    public class BudgetController : Controller  // BudgetController class that inherts from the controller Base class
                                                // The Controller base class provides methods like View(), RedirectToAction(), and Json() to handle different types of responses.
    {
        // Private fields to hold service references
        // Creates an object of I".."Service, that allows us to use the implementation methods of ".."Service methods
        // readonly ensures that _".."Service field can only be assigned once, guaranteeing that the field can not accidentally be changed later in the controller
        private readonly IBudgetService _budgetService; 
        private readonly IScheduleService _scheduleService;
        private readonly IToDoService _toDoService;
        private readonly ITransactionService _transactionService;
        private readonly IBillService _billService;

        // Constructor: Initializes the BudgetController with required services via dependency injection.
        public BudgetController(IBudgetService budgetService, IScheduleService scheduleService, IToDoService toDoService, ITransactionService transactionService, IBillService billService)
        {
            // Assign the I"..."Service instance provided by Dependency Injection to the private field _"..."Service.
            // This allows the controller to use the methods defined in the I"..."Service interface
            // (implemented by "..."Service) throughout the class.
            _budgetService = budgetService;            // Budget management
            _scheduleService = scheduleService;        // Schedule management
            _toDoService = toDoService;                // To-do task management
            _transactionService = transactionService;  // Transaction management
            _billService = billService;                // Bill management
        }


    // ***********
    // INDEX PAGE 
    // ***********

    // The Index method serves as the default page for the BudgetController.
    // It aggregates data from multiple services to provide an overview of budgets, tasks, schedules, and bills.
    // The data is packaged into a ViewModel and passed to the Index view.

    public async Task<IActionResult> Index()
    {
        // Fetch all budgets using the budget service.
        var budgets = await _budgetService.GetAllBudgetsAsync();

        // Fetch tasks for the current week using the schedule service.
        var tasksForWeek = await _scheduleService.GetTasksForCurrentWeekAsync();

        // Fetch today's tasks using the To-Do service.
        var todayTasks = await _toDoService.GetTodayTasksAsync();

        // Fetch daily schedules using the To-Do service.
        var dailySchedules = await _toDoService.GetDailySchedulesAsync();

        // Fetch bills for the current month using the bill service.
        // Filter the bills for the current month and year and sort them by due date.
        var currentMonth = DateTime.Now.Month; // Retrieve the current month.
        var currentYear = DateTime.Now.Year;  // Retrieve the current year.
        var monthlyBills = (await _billService.GetBillsForMonthAsync(currentMonth, currentYear))
            .OrderBy(b => b.DueDate) // Ensure bills are displayed in order of their due dates.
            .ToList(); // Convert the result to a list.

        // Create a ViewModel to combine all the retrieved data.
        // ViewModel is an object that acts as a container to organize and pass multiple pieces of data from a controller to a view.
        // BudgetWithTasksViewModel is a special type of model created specifically to pass data from the controller to the view.
        var viewModel = new BudgetWithTasksViewModel
        {
            Budgets = budgets,                  // List of all budgets.
            CurrentWeekTasks = tasksForWeek,    // Tasks for the current week.
            TodayTasks = todayTasks,            // Tasks for today.
            DailySchedules = dailySchedules,    // Daily schedules.
            MonthlyBills = monthlyBills         // Sorted list of monthly bills.
        };

        // Pass the ViewModel to the Index view for rendering.
        return View(viewModel);
    }







public async Task<IActionResult> Details(int id)
{
    if (id <= 0)
    {
        return NotFound("Invalid ID provided."); // Explicitly handle invalid IDs
    }

    try
    {
        var budget = await _budgetService.GetBudgetDetailsAsync(id);

        if (budget == null)
        {
            return NotFound();
        }
        budget.RecentTransactions = budget.Categories
            .SelectMany(c => c.Transactions)
            .OrderByDescending(t => t.Date)
            .Take(5) // Limit to the most recent 5 transactions
            .ToList();

        return View(budget);
    }
    catch (InvalidOperationException ex)
    {
        return NotFound($"An error occurred: {ex.Message}"); // Specific exception
    }
    catch (Exception ex)
    {
        // Handle generic exceptions
        return NotFound("An unexpected error occurred while fetching budget details.");
    }
}



[HttpGet]
public IActionResult Create()
{
    var budget = new Budget();
    return View(budget); // Pass the correct model
}



[HttpPost]
public async Task<IActionResult> Create(Budget budget)
{
    Console.WriteLine("Entering Create POST action."); // Debug: Method entry
    Console.WriteLine($"Received Budget Name: {budget.Name}");
    Console.WriteLine($"Received Budget Amount: {budget.TotalAmount}");

    if (!ModelState.IsValid)
    {
        Console.WriteLine("ModelState is invalid.");
        foreach (var error in ModelState)
        {
            Console.WriteLine($"Key: {error.Key}");
            foreach (var stateError in error.Value.Errors)
            {
                Console.WriteLine($"Error: {stateError.ErrorMessage}");
            }
        }
        return View(budget);
    }

    try
    {
        budget.DateCreated = DateTime.Now;
        Console.WriteLine("Attempting to create a new budget in the database.");
        await _budgetService.CreateBudgetAsync(budget);
        Console.WriteLine("Budget successfully created.");
        return RedirectToAction(nameof(Index));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Exception occurred: {ex.Message}");
        ModelState.AddModelError(string.Empty, ex.Message);
        return View(budget);
    }
}





        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var budget = await _budgetService.GetBudgetDetailsAsync(id);
                return View(budget);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Budget budget)
        {
            if (!ModelState.IsValid)
                return View(budget);

            try
            {
                await _budgetService.UpdateBudgetAsync(budget);
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _budgetService.DeleteBudgetAsync(id);

            if (!success)
                return NotFound("Budget not found.");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteScheduledTask(int id)
        {
            await _scheduleService.DeleteTaskAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> RecentTransactions()
        {
            var recentTransactions = await _transactionService.GetRecentTransactionsAsync();
            return PartialView("_RecentTransactions", recentTransactions);
        }


    }
}
