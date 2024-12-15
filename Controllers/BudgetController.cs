using BudgetTracker.Data;
using BudgetTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Controllers
{
    public class BudgetController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BudgetController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: List all budgets
        public IActionResult Index()
        {
            var budgets = _context.Budgets
                .Include(b => b.Categories) // Load categories for each budget
                .ToList();

            return View(budgets);
        }

        // GET: Create a new budget
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

                
        [HttpPost]
        public IActionResult Create(Budget budget)
        {
            // Validate TotalAmount for negative values
            if (budget.TotalAmount < 0)
            {
                ModelState.AddModelError("TotalAmount", "Total amount must be a positive value.");
                return View(budget); // Stay on the same view
            }

            if (_context.Budgets.Any(b => b.Name == budget.Name))
            {
                ModelState.AddModelError("Name", "A budget with the same name already exists.");
                return View(budget); // Return to the same view with the error
            }

            // Check model state validity
            if (ModelState.IsValid)
            {
                _context.Budgets.Add(budget);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View(budget); // Return view if model state is invalid
        }




        // GET: View details of a specific budget
        public IActionResult Details(int id)
        {
            var budget = _context.Budgets.FirstOrDefault(b => b.Id == id);
            if (budget == null) return NotFound();

            return View(budget);
        }

        // GET: Edit a budget
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var budget = _context.Budgets.FirstOrDefault(b => b.Id == id);
            if (budget == null) return NotFound();

            return View(budget);
        }

        // POST: Save the edited budget
        [HttpPost]
        public IActionResult Edit(Budget budget)
        {
            if (ModelState.IsValid)
            {
                _context.Budgets.Update(budget);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(budget);
        }

        // POST: Delete a budget
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var budget = _context.Budgets.FirstOrDefault(b => b.Id == id);
            if (budget == null) return NotFound();

            _context.Budgets.Remove(budget);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
