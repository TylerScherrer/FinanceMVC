using BudgetTracker.Interfaces;
using BudgetTracker.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BudgetTracker.Controllers
{
    public class BillController : Controller
    {
        private readonly IBillService _billService;

        public BillController(IBillService billService)
        {
            _billService = billService;
        }

        // GET: Bills
        public async Task<IActionResult> Index(int? budgetId)
        {
            if (budgetId == null)
            {
                return RedirectToAction("Index", "Budget"); // Redirect to Budget Index
            }

            try
            {
                var bills = await _billService.GetBillsAsync(budgetId.Value);
                ViewBag.BudgetId = budgetId;
                return View(bills);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching bills: {ex.Message}");
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Bills/Create
        [HttpGet]
        public IActionResult Create(int budgetId)
        {
            try
            {
                return View(new Bill { BudgetId = budgetId, DueDate = DateTime.Now });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading create bill page: {ex.Message}");
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: Bills/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Bill bill)
        {
            if (!ModelState.IsValid)
            {
                return View(bill);
            }

            try
            {
                await _billService.CreateBillAsync(bill);
                return RedirectToAction("Index", new { budgetId = bill.BudgetId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating bill: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while creating the bill.");
                return View(bill);
            }
        }

        // POST: Bills/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int budgetId)
        {
            try
            {
                var success = await _billService.DeleteBillAsync(id);
                if (!success)
                {
                    ModelState.AddModelError("", "Bill not found or could not be deleted.");
                }

                return RedirectToAction("Index", new { budgetId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting bill: {ex.Message}");
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewBills(int budgetId)
        {
            try
            {
                var bills = await _billService.GetBillsAsync(budgetId);
                ViewBag.BudgetId = budgetId;
                return View(bills); // Return a view with the bills list
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching bills: {ex.Message}");
                return RedirectToAction("Error", "Home");
            }
        }

    }
}