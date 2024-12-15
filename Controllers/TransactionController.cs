using BudgetTracker.Data;
using BudgetTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Controllers
{
    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransactionController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Create(int categoryId)
        {
            var transaction = new Transaction
            {
                CategoryId = categoryId
            };
            return View(transaction);
        }

        [HttpPost]
        public IActionResult Create(Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                // Fetch the category associated with the transaction
                var category = _context.Categories.FirstOrDefault(c => c.Id == transaction.CategoryId);

                if (category == null)
                {
                    return NotFound("Category not found.");
                }

                // Ensure the transaction amount does not exceed the remaining category amount
                if (category.AllocatedAmount < transaction.Amount)
                {
                    ModelState.AddModelError("", "Transaction amount exceeds the available category amount.");
                    return View(transaction);
                }

                // Subtract the transaction amount from the category
                category.AllocatedAmount -= transaction.Amount;

                // Save the new transaction
                _context.Transactions.Add(transaction);
                _context.SaveChanges();

                return RedirectToAction("Details", "Category", new { id = transaction.CategoryId });
            }

            return View(transaction);
        }
    [HttpPost]
    public IActionResult Delete(int id)
    {
        var transaction = _context.Transactions.Find(id);
        if (transaction != null)
        {
            _context.Transactions.Remove(transaction);
            _context.SaveChanges();
        }
        return RedirectToAction("Details", "Category", new { id = transaction.CategoryId }); 
        // Redirect back to the Category Details
    }

    }
}
