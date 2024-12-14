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
        Console.WriteLine($"Category Data: Name={category.Name}, AllocatedAmount={category.AllocatedAmount}, BudgetId={category.BudgetId}");

        if (ModelState.IsValid)
        {
            Console.WriteLine("Model is valid. Saving to database...");
            _context.Categories.Add(category);
            _context.SaveChanges();
            Console.WriteLine("Category saved successfully!");

            return RedirectToAction("Index", "Budget");
        }

        // Log validation errors
        Console.WriteLine("Model is invalid.");
        foreach (var key in ModelState.Keys)
        {
            var errors = ModelState[key].Errors;
            foreach (var error in errors)
            {
                Console.WriteLine($"Field: {key}, Error: {error.ErrorMessage}");
            }
        }

        return View(category);
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var category = _context.Categories
            .Include(c => c.Budget) // Load the related Budget for context if needed
            .FirstOrDefault(c => c.Id == id);

        if (category == null)
        {
            return NotFound("Category not found.");
        }

        return View(category);
    }

}
}