using BudgetTracker.Interfaces;
using BudgetTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.Controllers
{
    public class TransactionController : Controller
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
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
        public async Task<IActionResult> Create(Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _transactionService.CreateTransactionAsync(transaction);
                    return RedirectToAction("Details", "Category", new { id = transaction.CategoryId });
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            return View(transaction);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _transactionService.DeleteTransactionAsync(id);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            // Redirect back to the Category Details
            return RedirectToAction("Details", "Category", new { id = id });
        }
    }
}
