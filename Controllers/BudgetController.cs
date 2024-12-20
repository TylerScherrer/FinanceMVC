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

        public BudgetController(IBudgetService budgetService, IScheduleService scheduleService)
        {
            _budgetService = budgetService;
            _scheduleService = scheduleService;
        }
        
        public async Task<IActionResult> Index()
        {
            var budgets = await _budgetService.GetAllBudgetsAsync();

            // Fetch tasks for the current week
            var tasksForWeek = await _scheduleService.GetTasksForCurrentWeekAsync();

            // Populate the view model
            var viewModel = new BudgetWithTasksViewModel
            {
                Budgets = budgets,
                CurrentWeekTasks = tasksForWeek,
                TodayTasks = new List<ToDoItem>(), // Add logic if needed
                DailySchedules = new List<DailySchedule>() // Add logic if needed
            };

            return View(viewModel);
        }



        public async Task<IActionResult> Details(int id)
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

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Budget budget)
        {
            if (!ModelState.IsValid)
                return View(budget);

            await _budgetService.CreateBudgetAsync(budget);
            return RedirectToAction(nameof(Index));
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
    }
}
