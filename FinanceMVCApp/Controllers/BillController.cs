using BudgetTracker.Interfaces;
using BudgetTracker.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BudgetTracker.Controllers
{


    // ***********
    // BillController Class
    // ***********
    public class BillController : Controller // BillController class that inherts from the controller Base class
                                             // The Controller base class provides methods like View(), RedirectToAction(), and Json() to handle different types of responses.
    {
        private readonly IBillService _billService;  // Creates an object of IBillService, that allows us to use the implementation methods of BillService 
                                                     // readonly ensures that _billService field can only be assigned once, guaranteeing that the field can not accidentally be changed later in the controller
 


    // ***********
    // Constructor
    // ***********
    
    // The BillController is responsible for handling HTTP requests related to bills.
    // It relies on the IBillService interface to perform actions like fetching, creating, or deleting bills.
    public BillController(IBillService billService)
    {
        // Assign the IBillService instance provided by Dependency Injection to the private field _billService.
        // This allows the controller to use the methods defined in the IBillService interface
        // (implemented by BillService) throughout the class.
        _billService = billService;
    }




    // ***********
    // INDEX PAGE 
    // ***********

    // Returns a Task<IActionResult>, meaning it performs some asynchronous operation and eventually returns an IActionResult (like rendering a view or redirecting).   
    public async Task<IActionResult> Index(int? budgetId) // A nullable integrer representing the ID of a budget
                                                          // '?' allows budgetID to hold a value or be null
                                                          // This is useful when the parameter is optional
    {
        if (budgetId == null)
            {
                return RedirectToAction("Index", "Budget"); // Redirect to Budget Index
            }

            try
            {
                // await allows this method to free up the free thread to process other HTTP requests while data is being retreived 
                // Asynchronous call to free up server threads
                var bills = await _billService.GetBillsAsync(budgetId.Value); // Uses _billService to call GetBillsAsync to get the BudgetID values
                                                                              // Returns a list of bills for the given BudgetID from the database
                                                                              
                // Store the current budget ID in the ViewBag to pass it to the view.
                // Allows us to pass additional information (BudgetId) to the .cshtml view 
                // that is not part of the main model being passed (list of bills).
                ViewBag.BudgetId = budgetId;


                return View(bills); // Tells the controller to render the Index view and pass the bills list that was retrieved
            }

            // Catches any exceptions that may be thrown:
            // Error fetching the bills
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching bills: {ex.Message}"); // Logs the error for debugging
                return RedirectToAction("Error", "Home"); // Redirects the user to a generic error page to avoid crashes
            }
        }

        



    // ***********
    // GET METHOD FOR CREATE
    // ***********

    // GET endpoint for rendering the Create Bill form.
    // Initializes the Create form and pre-populates it with the provided budget ID and the current date as the due date.
    // The form allows the user to input details for adding a new bill associated with the specified budget.
    [HttpGet]
    public IActionResult Create(int budgetId) // Parameter: budgetId - The ID of the budget to which the new bill will be linked.
    {
        try
        {
            // Prepares a new Bill object with the given BudgetId and the current date as the default DueDate.
            // Passes the Bill object to the Create view to pre-fill form fields.
            return View(new Bill { BudgetId = budgetId, DueDate = DateTime.Now });
        }
        catch (Exception ex)
        {
            // Logs the error for debugging purposes (optional in production).
            Console.WriteLine($"Error initializing Create Bill form: {ex.Message}");
            
            // Redirects the user to a generic Error page to handle the exception gracefully.
            return RedirectToAction("Error", "Home");
        }
    }




    // ***********
    // POST Method for Creating a New Bill
    // ***********

    // Handles the form submission for creating a new bill.
    // Receives the newly created Bill object as a parameter from the user's form input.
    // Validates the data, saves it to the database, and redirects the user to the list of bills for the associated budget.
    // This is an asynchronous method that returns an IActionResult, which could be a view or a redirect depending on the operation's success or failure.
    [HttpPost]
    public async Task<IActionResult> Create(Bill bill)
    {
        if (!ModelState.IsValid)
        {
            // Check if the model state is invalid (e.g., validation rules failed).
            // If invalid, redisplay the form with the current data and validation messages.
            // This ensures the user can correct any errors and resubmit the form.
            return View(bill);
        }

        try
        {
            // Asynchronously calls the service to create the new bill.
            // 'await' allows this method to free up the current thread while the operation completes,
            // improving server performance by handling other requests concurrently.
            await _billService.CreateBillAsync(bill);  // Uses the Bill object passed from the form submission.

            // Redirects the user to the "ViewBills" page, displaying all bills for the same budget.
            return RedirectToAction("ViewBills", new { budgetId = bill.BudgetId });
        }
        catch (Exception ex)
        {
            // Logs the exception details to the server console for debugging purposes.
            // Includes the specific error message from the exception (ex.Message).
            Console.WriteLine($"Error creating bill: {ex.Message}");

            // Adds a model-level error to the ModelState to display a general error message to the user.
            // The first parameter ("") associates the error with the entire model rather than a specific field.
            ModelState.AddModelError("", "An error occurred while creating the bill.");

            // Returns the user back to the form view with the same Bill object.
            // This allows the user to see the error message and make any necessary corrections to their input.
            return View(bill);
        }
    }




    // ***********
    // POST Method for Deleting a Bill
    // ***********

    // This POST method handles the deletion of a specific bill from the database.
    // Parameters:
    // - `id`: The ID of the bill to be deleted.
    // - `budgetId`: The ID of the budget the bill belongs to.
    // Asynchronous operation ensures server responsiveness during the deletion process.
    [HttpPost]
    public async Task<IActionResult> Delete(int id, int budgetId)
    {
        try
        {
         
        // Attempt to delete the bill using the provided ID
        // DeleteBillAsynch returns True or False
        var success = await _billService.DeleteBillAsync(id); // Asynchronous service call to delete the bill

        // Check if the deletion was unsuccessful
        if (!success)
        {
            // Store an error message in TempData to inform the user
            TempData["Error"] = "Unable to delete the bill.";
        }

        // Redirect the user back to the bills page for the corresponding budget
        return RedirectToAction("ViewBills", new { budgetId });
        }
        catch (Exception ex)
        {
            // Log the error message to the console for debugging purposes
            Console.WriteLine($"Error deleting bill: {ex.Message}");

            // Store an error message in TempData to inform the user of the issue
            // TempData is used for passing message to the user, while consoleWrite is for server side logs 
            TempData["Error"] = "An error occurred while deleting the bill.";

            // Redirect the user back to the bills page for the corresponding budget
            return RedirectToAction("ViewBills", new { budgetId });
        }
    }



    // ***********
    // GET Method for View Bills
    // ***********

    // Retrieves and displays all bills associated with a specific budget.
    // Parameters:
    // - `budgetId`: The ID of the budget whose bills are being fetched.
    //
    // Uses asynchronous operations to fetch bills from the service layer.
    // Provides the retrieved bills and the budget ID to the view for display.
    [HttpGet]
    public async Task<IActionResult> ViewBills(int budgetId)
    {
        try
        {
            // Asynchronous call to the service to fetch bills for the given budget.
            var bills = await _billService.GetBillsAsync(budgetId);

            // Store the budget ID in the ViewBag for use in the view.
            ViewBag.BudgetId = budgetId;

            // Pass the bills to the Razor view for rendering.
            return View(bills);
        }
        catch (Exception ex)
        {
            // Log the error message to the console for debugging.
            Console.WriteLine($"Error retrieving bills for BudgetId {budgetId}: {ex.Message}");

            // Redirect the user to the error page if an exception occurs.
            return RedirectToAction("Error", "Home");
        }
    }





    // ***********
    // POST Method for Marking a Bill as Paid
    // ***********

    // Handles the marking of a specific bill as "paid" in the database.
    // Parameters:
    // - `id`: The ID of the bill to be updated.
    // - `budgetId`: The ID of the budget the bill belongs to, used for redirection.
    // Uses asynchronous operations to fetch bills from the service layer.
    [HttpPost]
    public async Task<IActionResult> MarkAsPaid(int id, int budgetId)
    {
        // Attempt to mark the bill as paid asynchronously.
        var success = await _billService.MarkAsPaidAsync(id);

        // If the operation fails, store an error message in TempData for the next request.
        if (!success)
        {
            TempData["Error"] = "Unable to mark the bill as paid.";
        }

        // Redirect the user back to the "ViewBills" page for the specified budget.
        return RedirectToAction("ViewBills", new { budgetId });
    }


    // ***********
    // GET Method for Editing a Bill
    // ***********

    // Handles displaying the Edit page for a specific bill.
    // Parameters:
    // - `id`: The ID of the bill to be edited.
    // Returns:
    // - A view containing the details of the bill to edit, or redirects if an error occurs.
    // Uses asynchronous operations to fetch bills from the service layer.
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            // Retrieve the bill with the specified ID.
            var bill = await _billService.GetBillByIdAsync(id);

            // Check if the bill exists. If it doesn't, store an error message in TempData.
            if (bill == null)
            {
                TempData["Error"] = "Bill not found."; // Inform the user that the bill couldn't be located.
                return RedirectToAction("Index", "Budget"); // Redirect to the Budget Index page.
            }

            // Pass the retrieved bill object to the Edit view for editing.
            return View(bill);
        }
        catch (Exception ex)
        {
            // Log the exception details to the console for debugging purposes.
            Console.WriteLine($"Error loading edit page: {ex.Message}");

            // Store a generic error message in TempData to inform the user.
            TempData["Error"] = "An error occurred while loading the edit page.";

            // Redirect to the Budget Index page.
            return RedirectToAction("Index", "Budget");
        }
    }


    // ***********
    // POST Method for Updating a Bill
    // ***********

    // Handles the form submission for updating an existing bill in the database.
    // Parameters:
    // - `bill`: The updated bill object submitted by the user.
    // Returns:
    // - Redirects to the appropriate page after updating, or returns to the Edit page if an error occurs.
    // Uses asynchronous operations to fetch bills from the service layer.
    [HttpPost]
    public async Task<IActionResult> Edit(Bill bill)
    {
        // Check if the model state is valid (e.g., all required fields are filled out correctly).
        if (!ModelState.IsValid)
        {
            // If validation fails, return to the Edit view with validation messages.
            return View(bill);
        }

        try
        {
            // Call the service layer to update the bill details in the database.
            await _billService.UpdateBillAsync(bill);

            // Redirect to the "ViewBills" page for the corresponding budget after a successful update.
            return RedirectToAction("ViewBills", new { budgetId = bill.BudgetId });
        }
        catch (Exception ex)
        {
            // Log the exception details to the console for debugging purposes.
            Console.WriteLine($"Error updating bill: {ex.Message}");

            // Add an error message to the ModelState to display it in the Edit view.
            ModelState.AddModelError("", "An error occurred while updating the bill.");

            // Return to the Edit view with the current bill details so the user can try again.
            return View(bill);
        }
    }




    // End of Controller 
    }
}