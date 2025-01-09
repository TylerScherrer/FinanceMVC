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
        [HttpGet]
        public async Task<IActionResult> Details(int categoryId)
        {
            var transactions = await _transactionService.GetTransactionsByCategoryAsync(categoryId);

            if (transactions == null)
            {
                return NotFound();
            }

            // Pass the transactions and category ID to the view
            ViewBag.CategoryId = categoryId;
            return View(transactions);
        }
[HttpGet]
public async Task<IActionResult> Edit(int id)
{
    var transaction = await _transactionService.GetTransactionByIdAsync(id);
    if (transaction == null)
    {
        return NotFound();
    }
    return View(transaction);
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(Transaction transaction)
{
    Console.WriteLine("[DEBUG] Received Transaction Data:");
    Console.WriteLine($"[DEBUG] Transaction ID: {transaction.Id}");
    Console.WriteLine($"[DEBUG] Description: {transaction.Description}");
    Console.WriteLine($"[DEBUG] Amount: {transaction.Amount}");
    Console.WriteLine($"[DEBUG] Date: {transaction.Date}");
    Console.WriteLine($"[DEBUG] Category ID: {transaction.CategoryId}");

    if (!ModelState.IsValid)
    {
        Console.WriteLine("[DEBUG] ModelState is invalid.");
        foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
        {
            Console.WriteLine($"[DEBUG] ModelState Error: {error.ErrorMessage}");
        }
        return View(transaction);
    }

    try
    {
        await _transactionService.UpdateTransactionAsync(transaction);
        Console.WriteLine("[DEBUG] Transaction updated successfully.");
        return RedirectToAction("Details", "Category", new { id = transaction.CategoryId });
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"[DEBUG] Exception: {ex.Message}");
        ModelState.AddModelError(string.Empty, ex.Message);
        return View(transaction);
    }
}



        
    }
}
