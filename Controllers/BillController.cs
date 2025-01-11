using BudgetTracker.Interfaces;
using BudgetTracker.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BudgetTracker.Controllers
{
    public class BillController : Controller // BillController class that inherts from the controller Base class
                                             // The Controller base class provides methods like View(), RedirectToAction(), and Json() to handle different types of responses.
    {
        private readonly IBillService _billService;  // Creates an object of IBillService, that allows us to use the implementation methods of BillService 
                                                     // readonly ensures that _billService field can only be assigned once, guaranteeing that the field can not accidentally be changed later in the controller
 
    // The BillController is responsible for handling HTTP requests related to bills.
    // It relies on the IBillService interface to perform actions like fetching, creating, or deleting bills.
    public BillController(IBillService billService)
    {
        // Assign the IBillService instance provided by Dependency Injection to the private field _billService.
        // This allows the controller to use the methods defined in the IBillService interface
        // (implemented by BillService) throughout the class.
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
                Console.WriteLine($"Initializing Create form for BudgetId={budgetId}");
                return View(new Bill { BudgetId = budgetId, DueDate = DateTime.Now }); // Pass BudgetId
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading create bill page: {ex.Message}");
                return RedirectToAction("Error", "Home");
            }
        }

[HttpPost]
public async Task<IActionResult> Create(Bill bill)
{
    if (!ModelState.IsValid)
    {
        // Return to the same page with validation messages if the model is invalid
        return View(bill);
    }

    try
    {
        await _billService.CreateBillAsync(bill);

        // Redirect to the "ViewBills" page for the same budget
        return RedirectToAction("ViewBills", new { budgetId = bill.BudgetId });
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
public async Task<IActionResult> Delete(int id, int budgetId)
{
    try
    {
        var success = await _billService.DeleteBillAsync(id);
        if (!success)
        {
            TempData["Error"] = "Unable to delete the bill.";
        }

        // Redirect back to the correct budget's bills page
        return RedirectToAction("ViewBills", new { budgetId });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error deleting bill: {ex.Message}");
        TempData["Error"] = "An error occurred while deleting the bill.";
        return RedirectToAction("ViewBills", new { budgetId });
    }
}


            [HttpGet]public async Task<IActionResult> ViewBills(int budgetId)
{
    if (budgetId == 0)
    {
        TempData["Error"] = "Invalid budget ID.";
        return RedirectToAction("Index", "Budget");
    }

    var bills = await _billService.GetBillsAsync(budgetId);
    ViewBag.BudgetId = budgetId;
    return View(bills);
}




[HttpPost]
public async Task<IActionResult> MarkAsPaid(int id, int budgetId)
{
    var success = await _billService.MarkAsPaidAsync(id);

    if (!success)
    {
        TempData["Error"] = "Unable to mark the bill as paid.";
    }

    return RedirectToAction("ViewBills", new { budgetId });
}

[HttpGet]
public async Task<IActionResult> Edit(int id)
{
    try
    {
        var bill = await _billService.GetBillByIdAsync(id);
        if (bill == null)
        {
            TempData["Error"] = "Bill not found.";
            return RedirectToAction("Index", "Budget");
        }

        return View(bill); // Pass the bill to the Edit view
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error loading edit page: {ex.Message}");
        TempData["Error"] = "An error occurred while loading the edit page.";
        return RedirectToAction("Index", "Budget");
    }
}

[HttpPost]
public async Task<IActionResult> Edit(Bill bill)
{
    if (!ModelState.IsValid)
    {
        // If validation fails, return to the Edit page with validation messages
        return View(bill);
    }

    try
    {
        await _billService.UpdateBillAsync(bill); // Update the bill
        return RedirectToAction("ViewBills", new { budgetId = bill.BudgetId });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error updating bill: {ex.Message}");
        ModelState.AddModelError("", "An error occurred while updating the bill.");
        return View(bill);
    }
}




    }
}