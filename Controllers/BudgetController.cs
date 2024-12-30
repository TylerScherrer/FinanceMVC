using BudgetTracker.Interfaces;
using BudgetTracker.Models;
using BudgetTracker.Services;
using Microsoft.AspNetCore.Mvc;


namespace BudgetTracker.Controllers
{
    public class BudgetController : Controller
    {
        private readonly IBudgetService _budgetService;
        private readonly IScheduleService _scheduleService;
        private readonly IToDoService _toDoService; // Add this line
        private readonly ITransactionService _transactionService;
        private readonly IBillService _billService;


        public BudgetController(IBudgetService budgetService, IScheduleService scheduleService, IToDoService toDoService, ITransactionService transactionService,IBillService billService)
        {
            _budgetService = budgetService;
            _scheduleService = scheduleService;
            _toDoService = toDoService; 
            _transactionService = transactionService;
            _billService = billService; // Assign the dependency to the private field
        }

        
        // In BudgetController
        public async Task<IActionResult> Index()
        {
            var budgets = await _budgetService.GetAllBudgetsAsync();
            var tasksForWeek = await _scheduleService.GetTasksForCurrentWeekAsync();

            // Fetch today's tasks and daily schedules
            var todayTasks = await _toDoService.GetTodayTasksAsync();
            var dailySchedules = await _toDoService.GetDailySchedulesAsync();

            // Fetch bills for the current month
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var monthlyBills = await _billService.GetBillsForMonthAsync(currentMonth, currentYear);

            var viewModel = new BudgetWithTasksViewModel
            {
                Budgets = budgets,
                CurrentWeekTasks = tasksForWeek,
                TodayTasks = todayTasks,
                DailySchedules = dailySchedules,
                MonthlyBills = monthlyBills // Add bills for the month
            };

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
