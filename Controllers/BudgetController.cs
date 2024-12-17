using BudgetTracker.Data;
using BudgetTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BudgetTracker.Extensions;


namespace BudgetTracker.Controllers
{
    public class BudgetController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BudgetController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var currentDate = DateTime.Now;

            // Fetch all budgets
            var budgets = _context.Budgets
                .Include(b => b.Categories) // Load related categories
                .ToList();

                // Update Remaining Amount dynamically

            // Fetch tasks for the current week
            var currentWeekTasks = _context.Tasks
                .Where(t => t.Date >= currentDate.StartOfWeek() && t.Date <= currentDate.EndOfWeek())
                .ToList();
            
                // Fetch To-Do tasks for today
            var todayTasks = _context.ToDoItems
                .Where(t => t.IsDaily || t.DueDate.Date == currentDate.Date)
                .ToList();

            // Combine into ViewModel
            var model = new BudgetWithTasksViewModel
            {
                Budgets = budgets,
                CurrentWeekTasks = currentWeekTasks,
                TodayTasks = todayTasks // Add today's To-Do tasks
            };

            return View(model); // Ensure the view receives BudgetWithTasksViewModel
        }


        // GET: Create a new budget
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

                
        [HttpPost]
public IActionResult Create(Category category)
{
    if (ModelState.IsValid)
    {
        // Fetch the budget
        var budget = _context.Budgets
                             .Include(b => b.Categories) // Include categories to calculate RemainingAmount
                             .FirstOrDefault(b => b.Id == category.BudgetId);

        if (budget == null)
        {
            return NotFound("Budget not found.");
        }

        // Validate allocated amount
        if (budget.RemainingAmount < category.AllocatedAmount)
        {
            ModelState.AddModelError("", "Allocated amount exceeds the remaining budget.");
            return View(category);
        }

        // Add the new category
        _context.Categories.Add(category);
        _context.SaveChanges();

        return RedirectToAction("Details", "Budget", new { id = category.BudgetId });
    }

    return View(category);
}





        // GET: View details of a specific budget
        public IActionResult Details(int id)
        {
            var budget = _context.Budgets.FirstOrDefault(b => b.Id == id);
            if (budget == null) return NotFound();

            return View(budget);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var budget = _context.Budgets.FirstOrDefault(b => b.Id == id);
            if (budget == null) return NotFound();
            return View(budget);
        }

        [HttpPost]
        public IActionResult Edit(Budget budget)
        {
            if (ModelState.IsValid)
            {
                _context.Budgets.Update(budget);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(budget);
        }


        [HttpPost]
        public IActionResult Delete(int id)
        {
            // Find the budget by ID
            var budget = _context.Budgets.Find(id);

            // Check if the budget exists
            if (budget != null)
            {
                _context.Budgets.Remove(budget);
                _context.SaveChanges();
            }

            // Redirect back to the Budget list (Index page)
            return RedirectToAction(nameof(Index));
        }

        

    }
}
