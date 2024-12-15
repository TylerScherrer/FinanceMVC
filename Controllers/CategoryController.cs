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

[HttpPost]
public IActionResult Create(Category category)
{
    if (ModelState.IsValid)
    {
        var budget = _context.Budgets
                             .Include(b => b.Categories)
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

        // Add the category (do not touch TotalAmount)
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

    [HttpPost]
    public IActionResult Delete(int id)
    {
        var category = _context.Categories.Find(id);
        if (category != null)
        {
            // Get the associated budget
            var budget = _context.Budgets.Find(category.BudgetId);

            if (budget != null)
            {
                // Adjust the TotalAmount or RemainingAmount of the budget
                

                // Update the budget in the database
                _context.Budgets.Update(budget);
            }

            // Remove the category
            _context.Categories.Remove(category);

            // Save all changes
            _context.SaveChanges();
        }

        return RedirectToAction("Index", "Budget");
    }



}
}