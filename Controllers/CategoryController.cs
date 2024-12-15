using Microsoft.AspNetCore.Mvc;
using BudgetTracker.Data;
using BudgetTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Controllers
{
public class CategoryController : Controller
{
    private readonly ApplicationDbContext _context;

    public CategoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Display the form to create a category
    [HttpGet]
    public IActionResult Create(int budgetId)
    {
        Console.WriteLine($"Category creation requested for BudgetId: {budgetId}");

        var category = new Category
        {
            BudgetId = budgetId // Pre-fill the BudgetId
        };

        return View(category);
    }

    // POST: Handle the form submission and save the category
    [HttpPost]
    public IActionResult Create(Category category)
    {
        if (ModelState.IsValid)
        {
            // Fetch the budget associated with the category
            var budget = _context.Budgets.FirstOrDefault(b => b.Id == category.BudgetId);

            if (budget == null)
            {
                return NotFound("Budget not found.");
            }

            // Ensure the allocated amount does not exceed the remaining budget
            if (budget.TotalAmount < category.AllocatedAmount)
            {
                ModelState.AddModelError("", "Allocated amount exceeds the available budget.");
                return View(category);
            }

            // Subtract the allocated amount from the budget
            budget.TotalAmount -= category.AllocatedAmount;

            // Save the new category
            _context.Categories.Add(category);
            _context.SaveChanges();

            return RedirectToAction("Details", "Budget", new { id = category.BudgetId });
        }

        return View(category);
    }


    [HttpGet]
    public IActionResult Details(int id)
    {
        var category = _context.Categories
            .Include(c => c.Budget) // Load the related Budget for context if needed
            .Include(c => c.Transactions) // Include related transactions
            .FirstOrDefault(c => c.Id == id);

        if (category == null)
        {
            return NotFound("Category not found.");
        }

        return View(category);
    }

}
}