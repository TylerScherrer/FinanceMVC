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

        public BudgetController(IBudgetService budgetService, IScheduleService scheduleService, IToDoService toDoService, ITransactionService transactionService)
        {
            _budgetService = budgetService;
            _scheduleService = scheduleService;
            _toDoService = toDoService; 
            _transactionService = transactionService;
        }

        
        // In BudgetController
        public async Task<IActionResult> Index()
        {
            var budgets = await _budgetService.GetAllBudgetsAsync();
            var tasksForWeek = await _scheduleService.GetTasksForCurrentWeekAsync();

            // Fetch today's tasks and daily schedules
            // Note: You'll need an instance of IToDoService or pass it into the constructor as well.
            var todayTasks = await _toDoService.GetTodayTasksAsync();
            var dailySchedules = await _toDoService.GetDailySchedulesAsync();

            var viewModel = new BudgetWithTasksViewModel
            {
                Budgets = budgets,
                CurrentWeekTasks = tasksForWeek,
                TodayTasks = todayTasks,
                DailySchedules = dailySchedules
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
            return NotFound("Budget not found."); // Handle case where no budget exists
        }

        // Safely fetch recent transactions and assign them to the budget
        budget.RecentTransactions = (await _transactionService.GetRecentTransactionsAsync()).ToList();

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
            return View();
        }

[HttpPost]
public async Task<IActionResult> Create(Budget budget)
{
    if (!ModelState.IsValid)
    {
        return View(budget);
    }

    try
    {
        budget.DateCreated = DateTime.Now;
        await _budgetService.CreateBudgetAsync(budget);
        return RedirectToAction(nameof(Index));
    }
    catch (Exception ex)
    {
        ModelState.AddModelError(string.Empty, ex.Message); // Correctly add the exception message
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
