using BudgetTracker.Controllers;       // Matches 'namespace BudgetTracker.Controllers'
using BudgetTracker.Interfaces;        // If IBudgetService etc. are in BudgetTracker.Interfaces
using BudgetTracker.Models;           // If Budget, etc. are in BudgetTracker.Models
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Tests // or choose any consistent test namespace
{
    /// <summary>
    /// This class contains a comprehensive suite of unit tests for the <see cref="BudgetController"/>.
    /// Each test follows the Arrange-Act-Assert pattern to verify specific functionalities
    /// of the BudgetController's endpoints, ensuring correctness, robustness, and reliability.
    /// </summary>
    public class BudgetControllerTests
    {
        /// <summary>
        /// Verifies that <c>Index</c> returns a <see cref="ViewResult"/> containing a 
        /// <see cref="BudgetWithTasksViewModel"/> with the expected budgets (and bills).
        /// </summary>
        [Fact]
        public async Task Index_ReturnsViewResult_WithViewModelContainingBudgets()
        {
            // ARRANGE
            var mockBudgetService = new Mock<IBudgetService>();
            var mockScheduleService = new Mock<IScheduleService>();
            var mockToDoService = new Mock<IToDoService>();
            var mockTransactionService = new Mock<ITransactionService>();
            var mockBillService = new Mock<IBillService>();

            // Mock the budget service to return a list with a single Budget.
            mockBudgetService
                .Setup(svc => svc.GetAllBudgetsAsync())
                .ReturnsAsync(new List<Budget>
                {
                    new Budget { Id = 1, Name = "Test Budget 1" }
                });

            // Mock the bill service to return a list with a single Bill.
            mockBillService
                .Setup(svc => svc.GetBillsForMonthAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<Bill>
                {
                    new Bill { Id = 100, Name = "Test Bill", DueDate = DateTime.Now }
                });

            // Create an instance of the controller with all dependencies.
            var controller = new BudgetController(
                mockBudgetService.Object,
                mockScheduleService.Object,
                mockToDoService.Object,
                mockTransactionService.Object,
                mockBillService.Object
            );

            // ACT
            var result = await controller.Index();

            // ASSERT
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<BudgetWithTasksViewModel>(viewResult.Model);

            // Confirm the budgets count and bills count in the returned model.
            Assert.Single(model.Budgets);
            Assert.NotNull(model.MonthlyBills);
            Assert.Single(model.MonthlyBills);
        }

        /// <summary>
        /// Verifies that <c>Details</c> returns a valid <see cref="ViewResult"/> containing the 
        /// correct <see cref="Budget"/> when a valid ID is provided.
        /// </summary>
        [Fact]
        public async Task Details_ValidId_ReturnsViewResultWithBudget()
        {
            // ARRANGE
            var mockBudgetService = new Mock<IBudgetService>();
            var mockScheduleService = new Mock<IScheduleService>();
            var mockToDoService = new Mock<IToDoService>();
            var mockTransactionService = new Mock<ITransactionService>();
            var mockBillService = new Mock<IBillService>();

            var testBudget = new Budget
            {
                Id = 1,
                Name = "Test Budget",
                Categories = new List<Category>
                {
                    new Category
                    {
                        Transactions = new List<Transaction>
                        {
                            new Transaction { Date = DateTime.Now, Amount = 100 }
                        }
                    }
                }
            };

            mockBudgetService
                .Setup(svc => svc.GetBudgetDetailsAsync(1))
                .ReturnsAsync(testBudget);

            var controller = new BudgetController(
                mockBudgetService.Object,
                mockScheduleService.Object,
                mockToDoService.Object,
                mockTransactionService.Object,
                mockBillService.Object
            );

            // ACT
            var result = await controller.Details(1);

            // ASSERT
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Budget>(viewResult.Model);

            Assert.Equal(testBudget.Id, model.Id);
            Assert.Single(model.Categories.First().Transactions);
        }

        /// <summary>
        /// Verifies that creating a valid <see cref="Budget"/> redirects to the 
        /// <c>Index</c> action successfully.
        /// </summary>
        [Fact]
        public async Task Create_ValidModel_RedirectsToIndex()
        {
            // ARRANGE
            var mockBudgetService = new Mock<IBudgetService>();
            var mockScheduleService = new Mock<IScheduleService>();
            var mockToDoService = new Mock<IToDoService>();
            var mockTransactionService = new Mock<ITransactionService>();
            var mockBillService = new Mock<IBillService>();

            var newBudget = new Budget
            {
                Name = "New Budget",
                TotalAmount = 1000
            };

            mockBudgetService
                .Setup(svc => svc.CreateBudgetAsync(newBudget))
                .ReturnsAsync(newBudget);

            var controller = new BudgetController(
                mockBudgetService.Object,
                mockScheduleService.Object,
                mockToDoService.Object,
                mockTransactionService.Object,
                mockBillService.Object
            );

            // ACT
            var result = await controller.Create(newBudget);

            // ASSERT
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(controller.Index), redirectResult.ActionName);
        }

        /// <summary>
        /// Ensures that when attempting to create a <see cref="Budget"/> with an invalid model state,
        /// the user is returned to the <c>Create</c> view containing the same model for correction.
        /// </summary>
        [Fact]
        public async Task Create_InvalidModel_ReturnsViewWithModel()
        {
            // ARRANGE
            var mockBudgetService = new Mock<IBudgetService>();
            var mockScheduleService = new Mock<IScheduleService>();
            var mockToDoService = new Mock<IToDoService>();
            var mockTransactionService = new Mock<ITransactionService>();
            var mockBillService = new Mock<IBillService>();

            var invalidBudget = new Budget
            {
                // Invalid because the Name is required but left empty
                Name = ""
            };

            var controller = new BudgetController(
                mockBudgetService.Object,
                mockScheduleService.Object,
                mockToDoService.Object,
                mockTransactionService.Object,
                mockBillService.Object
            );

            // Simulate a model validation error.
            controller.ModelState.AddModelError("Name", "The Name field is required.");

            // ACT
            var result = await controller.Create(invalidBudget);

            // ASSERT
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(invalidBudget, viewResult.Model);
        }

        /// <summary>
        /// Verifies that the <c>Delete</c> action redirects to the <c>Index</c> view 
        /// when a valid budget ID is provided.
        /// </summary>
        [Fact]
        public async Task Delete_ValidId_RedirectsToIndex()
        {
            // ARRANGE
            var mockBudgetService = new Mock<IBudgetService>();
            var mockScheduleService = new Mock<IScheduleService>();
            var mockToDoService = new Mock<IToDoService>();
            var mockTransactionService = new Mock<ITransactionService>();
            var mockBillService = new Mock<IBillService>();

            mockBudgetService
                .Setup(svc => svc.DeleteBudgetAsync(1))
                .ReturnsAsync(true);

            var controller = new BudgetController(
                mockBudgetService.Object,
                mockScheduleService.Object,
                mockToDoService.Object,
                mockTransactionService.Object,
                mockBillService.Object
            );

            // ACT
            var result = await controller.Delete(1);

            // ASSERT
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(controller.Index), redirectResult.ActionName);
        }

        /// <summary>
        /// Validates that if an invalid ID is supplied to <c>Delete</c>, 
        /// a <see cref="NotFoundObjectResult"/> is returned.
        /// </summary>
        [Fact]
        public async Task Delete_InvalidId_ReturnsNotFound()
        {
            // ARRANGE
            var mockBudgetService = new Mock<IBudgetService>();
            var mockScheduleService = new Mock<IScheduleService>();
            var mockToDoService = new Mock<IToDoService>();
            var mockTransactionService = new Mock<ITransactionService>();
            var mockBillService = new Mock<IBillService>();

            mockBudgetService
                .Setup(svc => svc.DeleteBudgetAsync(It.IsAny<int>()))
                .ReturnsAsync(false);

            var controller = new BudgetController(
                mockBudgetService.Object,
                mockScheduleService.Object,
                mockToDoService.Object,
                mockTransactionService.Object,
                mockBillService.Object
            );

            // ACT
            var result = await controller.Delete(-1);

            // ASSERT
            Assert.IsType<NotFoundObjectResult>(result);
        }

        /// <summary>
        /// Ensures that <c>Index</c> handles an empty budget list without errors, 
        /// returning a <see cref="BudgetWithTasksViewModel"/> with empty collections.
        /// </summary>
        [Fact]
        public async Task Index_NoBudgets_ReturnsViewResultWithEmptyBudgets()
        {
            // ARRANGE
            var mockBudgetService = new Mock<IBudgetService>();
            var mockBillService = new Mock<IBillService>();

            mockBudgetService
                .Setup(svc => svc.GetAllBudgetsAsync())
                .ReturnsAsync(new List<Budget>());

            mockBillService
                .Setup(svc => svc.GetBillsForMonthAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<Bill>());

            var controller = new BudgetController(
                mockBudgetService.Object,
                Mock.Of<IScheduleService>(),
                Mock.Of<IToDoService>(),
                Mock.Of<ITransactionService>(),
                mockBillService.Object
            );

            // ACT
            var result = await controller.Index();

            // ASSERT
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<BudgetWithTasksViewModel>(viewResult.Model);

            Assert.Empty(model.Budgets);
            Assert.Empty(model.MonthlyBills);
        }

        /// <summary>
        /// Validates that if an exception is thrown during <c>Create</c>, the view is returned with the 
        /// appropriate error message in the <see cref="ModelState"/>.
        /// </summary>
        [Fact]
        public async Task Create_ThrowsException_ReturnsViewWithError()
        {
            // ARRANGE
            var mockBudgetService = new Mock<IBudgetService>();
            mockBudgetService
                .Setup(svc => svc.CreateBudgetAsync(It.IsAny<Budget>()))
                .ThrowsAsync(new Exception("Database error"));

            var controller = new BudgetController(
                mockBudgetService.Object,
                Mock.Of<IScheduleService>(),
                Mock.Of<IToDoService>(),
                Mock.Of<ITransactionService>(),
                Mock.Of<IBillService>()
            );

            var budget = new Budget { Id = 1, Name = "Test Budget" };

            // ACT
            var result = await controller.Create(budget);

            // ASSERT
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Database error", controller.ModelState[string.Empty]?.Errors.FirstOrDefault()?.ErrorMessage);
        }

        /// <summary>
        /// Confirms that <c>Index</c> can successfully handle partial data, 
        /// e.g., budgets existing but no bills found, without throwing an error.
        /// </summary>
        [Fact]
        public async Task Index_PartialData_ReturnsViewResult()
        {
            // ARRANGE
            var mockBudgetService = new Mock<IBudgetService>();
            var mockBillService = new Mock<IBillService>();

            mockBudgetService
                .Setup(svc => svc.GetAllBudgetsAsync())
                .ReturnsAsync(new List<Budget>
                {
                    new Budget { Id = 1, Name = "Budget 1" }
                });

            // No bills returned
            mockBillService
                .Setup(svc => svc.GetBillsForMonthAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<Bill>());

            var controller = new BudgetController(
                mockBudgetService.Object,
                Mock.Of<IScheduleService>(),
                Mock.Of<IToDoService>(),
                Mock.Of<ITransactionService>(),
                mockBillService.Object
            );

            // ACT
            var result = await controller.Index();

            // ASSERT
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<BudgetWithTasksViewModel>(viewResult.Model);

            Assert.Single(model.Budgets);
            Assert.Empty(model.MonthlyBills);
        }

        /// <summary>
        /// Checks that <c>RecentTransactions</c> returns a partial view and that it contains
        /// the expected transactions from the service.
        /// </summary>
        [Fact]
        public async Task RecentTransactions_ReturnsPartialViewWithTransactions()
        {
            // ARRANGE
            var mockTransactionService = new Mock<ITransactionService>();
            mockTransactionService
                .Setup(svc => svc.GetRecentTransactionsAsync())
                .ReturnsAsync(new List<Transaction>
                {
                    new Transaction { Date = DateTime.Now, Amount = 100 }
                });

            var controller = new BudgetController(
                Mock.Of<IBudgetService>(),
                Mock.Of<IScheduleService>(),
                Mock.Of<IToDoService>(),
                mockTransactionService.Object,
                Mock.Of<IBillService>()
            );

            // ACT
            var result = await controller.RecentTransactions();

            // ASSERT
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Transaction>>(partialViewResult.Model);
            Assert.Single(model);
        }

        /// <summary>
        /// Ensures that the <c>Details</c> method populates <c>RecentTransactions</c> 
        /// with the latest transactions from the budget’s categories.
        /// </summary>
        [Fact]
        public async Task Details_PopulatesRecentTransactions()
        {
            // ARRANGE
            var mockBudgetService = new Mock<IBudgetService>();
            var budget = new Budget
            {
                Id = 1,
                Name = "Budget with Transactions",
                Categories = new List<Category>
                {
                    new Category
                    {
                        Transactions = new List<Transaction>
                        {
                            new Transaction { Date = DateTime.Now, Amount = 100 },
                            new Transaction { Date = DateTime.Now.AddDays(-1), Amount = 50 }
                        }
                    }
                }
            };

            mockBudgetService
                .Setup(svc => svc.GetBudgetDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync(budget);

            var controller = new BudgetController(
                mockBudgetService.Object,
                Mock.Of<IScheduleService>(),
                Mock.Of<IToDoService>(),
                Mock.Of<ITransactionService>(),
                Mock.Of<IBillService>()
            );

            // ACT
            var result = await controller.Details(1);

            // ASSERT
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Budget>(viewResult.Model);
            Assert.Equal(2, model.RecentTransactions.Count);
        }

        /// <summary>
        /// Verifies that if <c>GetBudgetDetailsAsync</c> returns <c>null</c>, 
        /// the <c>Details</c> action responds with a <see cref="NotFoundResult"/>.
        /// </summary>
        [Fact]
        public async Task Details_NullBudget_ReturnsNotFound()
        {
            // ARRANGE
            var mockBudgetService = new Mock<IBudgetService>();
            mockBudgetService
                .Setup(svc => svc.GetBudgetDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync((Budget)null!); // null for demonstration

            var controller = new BudgetController(
                mockBudgetService.Object,
                Mock.Of<IScheduleService>(),
                Mock.Of<IToDoService>(),
                Mock.Of<ITransactionService>(),
                Mock.Of<IBillService>()
            );

            // ACT
            var result = await controller.Details(1);

            // ASSERT
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Tests the <c>Edit</c> action when <see cref="GetBudgetDetailsAsync"/> returns <c>null</c>;
        /// it should produce a <see cref="NotFoundResult"/>.
        /// </summary>
        [Fact]
        public async Task Edit_NullBudget_ReturnsNotFound()
        {
            // ARRANGE
            var mockBudgetService = new Mock<IBudgetService>();
            mockBudgetService
                .Setup(svc => svc.GetBudgetDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync((Budget)null!); // null for demonstration

            var controller = new BudgetController(
                mockBudgetService.Object,
                Mock.Of<IScheduleService>(),
                Mock.Of<IToDoService>(),
                Mock.Of<ITransactionService>(),
                Mock.Of<IBillService>()
            );

            // ACT
            var result = await controller.Edit(1);

            // ASSERT
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Validates that a valid task ID passed to <c>DeleteScheduledTask</c> 
        /// redirects to the <c>Index</c> action on success.
        /// </summary>
        [Fact]
        public async Task DeleteScheduledTask_ValidId_RedirectsToIndex()
        {
            // ARRANGE
            var mockScheduleService = new Mock<IScheduleService>();
            mockScheduleService
                .Setup(svc => svc.DeleteTaskAsync(It.IsAny<int>()))
                .ReturnsAsync(true); // Return success

            var controller = new BudgetController(
                Mock.Of<IBudgetService>(),
                mockScheduleService.Object,
                Mock.Of<IToDoService>(),
                Mock.Of<ITransactionService>(),
                Mock.Of<IBillService>()
            );

            // ACT
            var result = await controller.DeleteScheduledTask(1);

            // ASSERT
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        /// <summary>
        /// Verifies that if an invalid model is provided to the <c>Edit</c> action, 
        /// the user is returned to the <c>Edit</c> view containing the same <see cref="Budget"/>.
        /// </summary>
        [Fact]
        public async Task Edit_InvalidModel_ReturnsViewWithModel()
        {
            // ARRANGE
            var mockBudgetService = new Mock<IBudgetService>();
            var controller = new BudgetController(
                mockBudgetService.Object,
                Mock.Of<IScheduleService>(),
                Mock.Of<IToDoService>(),
                Mock.Of<ITransactionService>(),
                Mock.Of<IBillService>()
            );

            var invalidBudget = new Budget { Name = "" }; // Invalid: Name is required
            controller.ModelState.AddModelError("Name", "The Name field is required.");

            // ACT
            var result = await controller.Edit(invalidBudget);

            // ASSERT
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(invalidBudget, viewResult.Model);
        }

        /// <summary>
        /// Checks that if <c>DeleteScheduledTask</c> is called with an invalid ID and fails,
        /// the user is redirected to the <c>Index</c> action without performing any deletion.
        /// </summary>
        [Fact]
        public async Task DeleteScheduledTask_InvalidId_ReturnsRedirectToIndex()
        {
            // ARRANGE
            var mockScheduleService = new Mock<IScheduleService>();
            mockScheduleService
                .Setup(svc => svc.DeleteTaskAsync(It.IsAny<int>()))
                .ReturnsAsync(false); // Simulate failure

            var controller = new BudgetController(
                Mock.Of<IBudgetService>(),
                mockScheduleService.Object,
                Mock.Of<IToDoService>(),
                Mock.Of<ITransactionService>(),
                Mock.Of<IBillService>()
            );

            // ACT
            var result = await controller.DeleteScheduledTask(-1);

            // ASSERT
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        /// <summary>
        /// Verifies that <c>RecentTransactions</c> returns an empty list if no recent transactions are available.
        /// </summary>
        [Fact]
        public async Task RecentTransactions_NoTransactions_ReturnsEmptyList()
        {
            // ARRANGE
            var mockTransactionService = new Mock<ITransactionService>();
            mockTransactionService
                .Setup(svc => svc.GetRecentTransactionsAsync())
                .ReturnsAsync(new List<Transaction>());

            var controller = new BudgetController(
                Mock.Of<IBudgetService>(),
                Mock.Of<IScheduleService>(),
                Mock.Of<IToDoService>(),
                mockTransactionService.Object,
                Mock.Of<IBillService>()
            );

            // ACT
            var result = await controller.RecentTransactions();

            // ASSERT
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Transaction>>(partialViewResult.Model);
            Assert.Empty(model);
        }

        /// <summary>
        /// Verifies that if an exception is thrown in <c>Create</c>, the proper error message is 
        /// surfaced in the <see cref="ModelState"/> and the user is returned to the <c>Create</c> view.
        /// </summary>
        [Fact]
        public async Task Create_ThrowsException_ReturnsViewWithErrorMessage()
        {
            // ARRANGE
            var mockBudgetService = new Mock<IBudgetService>();
            mockBudgetService
                .Setup(svc => svc.CreateBudgetAsync(It.IsAny<Budget>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            var controller = new BudgetController(
                mockBudgetService.Object,
                Mock.Of<IScheduleService>(),
                Mock.Of<IToDoService>(),
                Mock.Of<ITransactionService>(),
                Mock.Of<IBillService>()
            );

            var budget = new Budget { Name = "Test Budget", TotalAmount = 500 };

            // ACT
            var result = await controller.Create(budget);

            // ASSERT
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Unexpected error", controller.ModelState[string.Empty]?.Errors.FirstOrDefault()?.ErrorMessage);
        }

        /// <summary>
        /// Ensures that <c>Index</c> returns an empty view model when both 
        /// budgets and bills are empty.
        /// </summary>
        [Fact]
        public async Task Index_EmptyData_ReturnsEmptyViewModel()
        {
            // ARRANGE
            var mockBudgetService = new Mock<IBudgetService>();
            mockBudgetService
                .Setup(svc => svc.GetAllBudgetsAsync())
                .ReturnsAsync(new List<Budget>());

            var mockBillService = new Mock<IBillService>();
            mockBillService
                .Setup(svc => svc.GetBillsForMonthAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<Bill>());

            var controller = new BudgetController(
                mockBudgetService.Object,
                Mock.Of<IScheduleService>(),
                Mock.Of<IToDoService>(),
                Mock.Of<ITransactionService>(),
                mockBillService.Object
            );

            // ACT
            var result = await controller.Index();

            // ASSERT
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<BudgetWithTasksViewModel>(viewResult.Model);

            Assert.Empty(model.Budgets);
            Assert.Empty(model.MonthlyBills);
        }

        /// <summary>
        /// Tests that <c>Index</c> returns a view model containing bills sorted by their due date.
        /// </summary>
        [Fact]
        public async Task Index_ReturnsViewResult_WithBillsSortedByDueDate()
        {
            // ARRANGE
            var mockBudgetService = new Mock<IBudgetService>();
            var mockBillService = new Mock<IBillService>();

            mockBudgetService
                .Setup(svc => svc.GetAllBudgetsAsync())
                .ReturnsAsync(new List<Budget>
                {
                    new Budget { Id = 1, Name = "Budget 1" }
                });

            mockBillService
                .Setup(svc => svc.GetBillsForMonthAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<Bill>
                {
                    new Bill { Id = 1, Name = "Bill 1", DueDate = DateTime.Now.AddDays(5) },
                    new Bill { Id = 2, Name = "Bill 2", DueDate = DateTime.Now.AddDays(1) },
                    new Bill { Id = 3, Name = "Bill 3", DueDate = DateTime.Now.AddDays(10) }
                });

            var controller = new BudgetController(
                mockBudgetService.Object,
                Mock.Of<IScheduleService>(),
                Mock.Of<IToDoService>(),
                Mock.Of<ITransactionService>(),
                mockBillService.Object
            );

            // ACT
            var result = await controller.Index();

            // ASSERT
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<BudgetWithTasksViewModel>(viewResult.Model);

            Assert.Equal(3, model.MonthlyBills.Count);
            Assert.Equal("Bill 2", model.MonthlyBills[0].Name); 
            Assert.Equal("Bill 1", model.MonthlyBills[1].Name);
            Assert.Equal("Bill 3", model.MonthlyBills[2].Name);
        }

        /// <summary>
        /// Verifies that the <c>Details</c> method handles a <see cref="Budget"/> with no transactions 
        /// gracefully, returning an empty list of transactions.
        /// </summary>
        [Fact]
        public async Task Details_BudgetWithNoTransactions_ReturnsViewWithEmptyTransactions()
        {
            // ARRANGE
            var mockBudgetService = new Mock<IBudgetService>();
            mockBudgetService
                .Setup(svc => svc.GetBudgetDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync(new Budget
                {
                    Id = 1,
                    Name = "Test Budget",
                    Categories = new List<Category>
                    {
                        new Category { Id = 1, Name = "Category 1", Transactions = new List<Transaction>() }
                    }
                });

            var controller = new BudgetController(
                mockBudgetService.Object,
                Mock.Of<IScheduleService>(),
                Mock.Of<IToDoService>(),
                Mock.Of<ITransactionService>(),
                Mock.Of<IBillService>()
            );

            // ACT
            var result = await controller.Details(1);

            // ASSERT
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Budget>(viewResult.Model);

            Assert.Empty(model.Categories.First().Transactions);
        }

        /// <summary>
        /// Tests that attempting to create a <see cref="Budget"/> with a negative <c>TotalAmount</c>
        /// triggers the appropriate validation error.
        /// </summary>
        [Fact]
        public async Task Create_BudgetWithNegativeTotalAmount_ReturnsError()
        {
            // ARRANGE
            var mockBudgetService = new Mock<IBudgetService>();
            var controller = new BudgetController(
                mockBudgetService.Object,
                Mock.Of<IScheduleService>(),
                Mock.Of<IToDoService>(),
                Mock.Of<ITransactionService>(),
                Mock.Of<IBillService>()
            );

            var invalidBudget = new Budget { Name = "Invalid Budget", TotalAmount = -100 };

            // ACT
            var result = await controller.Create(invalidBudget);

            // ASSERT
            var viewResult = Assert.IsType<ViewResult>(result); 
            Assert.False(controller.ModelState.IsValid);
            Assert.Equal("Total amount must be greater than or equal to zero.",
                controller.ModelState["TotalAmount"].Errors.FirstOrDefault()?.ErrorMessage);
        }

        /// <summary>
        /// Checks that deleting a budget with a valid ID successfully removes the budget
        /// from the list of budgets displayed on the <c>Index</c> page.
        /// </summary>
        [Fact]
        public async Task Delete_ValidId_RemovesBudgetFromIndex()
        {
            // ARRANGE
            var mockBudgetService = new Mock<IBudgetService>();
            var budgets = new List<Budget>
            {
                new Budget { Id = 1, Name = "Budget 1" }
            };

            mockBudgetService
                .Setup(svc => svc.DeleteBudgetAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) =>
                {
                    budgets.RemoveAll(b => b.Id == id);
                    return true;
                });

            mockBudgetService
                .Setup(svc => svc.GetAllBudgetsAsync())
                .ReturnsAsync(() => budgets);

            var controller = new BudgetController(
                mockBudgetService.Object,
                Mock.Of<IScheduleService>(),
                Mock.Of<IToDoService>(),
                Mock.Of<ITransactionService>(),
                Mock.Of<IBillService>()
            );

            // ACT
            var deleteResult = await controller.Delete(1);
            var indexResult = await controller.Index();

            // ASSERT
            var redirectResult = Assert.IsType<RedirectToActionResult>(deleteResult);
            Assert.Equal("Index", redirectResult.ActionName);

            var viewResult = Assert.IsType<ViewResult>(indexResult);
            var model = Assert.IsAssignableFrom<BudgetWithTasksViewModel>(viewResult.Model);
            Assert.Empty(model.Budgets);
        }

        /// <summary>
        /// Ensures that when editing a <see cref="Budget"/> with valid data, 
        /// the update operation succeeds and the user is redirected to <c>Index</c>.
        /// </summary>
        [Fact]
        public async Task Edit_ValidModel_UpdatesBudgetSuccessfully()
        {
            // ARRANGE
            var mockBudgetService = new Mock<IBudgetService>();
            var updatedBudget = new Budget { Id = 1, Name = "Updated Budget", TotalAmount = 500 };

            mockBudgetService
                .Setup(svc => svc.UpdateBudgetAsync(It.IsAny<Budget>()))
                .ReturnsAsync(updatedBudget);

            var controller = new BudgetController(
                mockBudgetService.Object,
                Mock.Of<IScheduleService>(),
                Mock.Of<IToDoService>(),
                Mock.Of<ITransactionService>(),
                Mock.Of<IBillService>()
            );

            // ACT
            var result = await controller.Edit(updatedBudget);

            // ASSERT
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            mockBudgetService.Verify(svc => svc.UpdateBudgetAsync(updatedBudget), Times.Once);
        }

        /// <summary>
        /// Ensures that attempting to create a <see cref="Budget"/> with an excessively long name 
        /// results in a validation error and the user is returned to the <c>Create</c> view.
        /// </summary>
        [Fact]
        public async Task Create_ExcessivelyLongName_ReturnsValidationError()
        {
            // ARRANGE
            var mockBudgetService = new Mock<IBudgetService>();
            var controller = new BudgetController(
                mockBudgetService.Object,
                Mock.Of<IScheduleService>(),
                Mock.Of<IToDoService>(),
                Mock.Of<ITransactionService>(),
                Mock.Of<IBillService>()
            );

            // Name exceeding maximum length for demonstration
            var invalidBudget = new Budget { Name = new string('A', 256) };
            controller.ModelState.AddModelError("Name", "The Name field cannot exceed 255 characters.");

            // ACT
            var result = await controller.Create(invalidBudget);

            // ASSERT
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.Contains(
                controller.ModelState["Name"].Errors,
                e => e.ErrorMessage.Contains("cannot exceed 255 characters")
            );
        }

        /// <summary>
        /// Verifies that a budget with multiple transactions in its categories correctly aggregates them
        /// in the <c>RecentTransactions</c> property, displayed in descending order by date.
        /// </summary>
        [Fact]
        public async Task Details_BudgetWithMultipleTransactions_ReturnsViewWithAggregatedTransactions()
        {
            // ARRANGE
            var mockBudgetService = new Mock<IBudgetService>();
            var budget = new Budget
            {
                Id = 1,
                Name = "Budget with Transactions",
                Categories = new List<Category>
                {
                    new Category
                    {
                        Transactions = new List<Transaction>
                        {
                            new Transaction { Date = DateTime.Now, Amount = 100 },
                            new Transaction { Date = DateTime.Now.AddDays(-1), Amount = 50 },
                            new Transaction { Date = DateTime.Now.AddDays(-2), Amount = 75 }
                        }
                    }
                }
            };

            mockBudgetService
                .Setup(svc => svc.GetBudgetDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync(budget);

            var controller = new BudgetController(
                mockBudgetService.Object,
                Mock.Of<IScheduleService>(),
                Mock.Of<IToDoService>(),
                Mock.Of<ITransactionService>(),
                Mock.Of<IBillService>()
            );

            // ACT
            var result = await controller.Details(1);

            // ASSERT
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Budget>(viewResult.Model);
            Assert.Equal(3, model.RecentTransactions.Count);
        }

        /// <summary>
        /// Verifies that the <c>Edit</c> action returns a <c>ViewResult</c> with an appropriate error
        /// when a <see cref="DbUpdateConcurrencyException"/> (i.e., concurrency conflict) is thrown.
        /// </summary>
        [Fact]
        public async Task Edit_ConcurrencyConflict_ReturnsErrorMessage()
        {
            // ARRANGE
            var mockBudgetService = new Mock<IBudgetService>();
            var budget = new Budget { Id = 1, Name = "Test Budget" };

            // Simulate concurrency conflict
            mockBudgetService
                .Setup(s => s.UpdateBudgetAsync(It.IsAny<Budget>()))
                .ThrowsAsync(new DbUpdateConcurrencyException());

            var controller = new BudgetController(
                mockBudgetService.Object, 
                null, 
                null, 
                null, 
                null
            );

            // ACT
            var result = await controller.Edit(budget) as ViewResult;

            // ASSERT
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
            Assert.True(controller.ViewData.ModelState.ErrorCount > 0);
            Assert.Contains("Concurrency conflict occurred", 
                controller.ViewData.ModelState[string.Empty].Errors[0].ErrorMessage);
        }

        /// <summary>
        /// Ensures that if no tasks or schedules are returned by their respective services, 
        /// <c>Index</c> still returns a valid <see cref="BudgetWithTasksViewModel"/> 
        /// with empty collections.
        /// </summary>
        [Fact]
        public async Task Index_NoTasksOrSchedules_ReturnsEmptyViewModel()
        {
            // ARRANGE
            var mockBudgetService = new Mock<IBudgetService>();
            var mockScheduleService = new Mock<IScheduleService>();
            var mockToDoService = new Mock<IToDoService>();
            var mockTransactionService = new Mock<ITransactionService>();
            var mockBillService = new Mock<IBillService>();

            // Mocking all return values to be empty
            mockBudgetService.Setup(s => s.GetAllBudgetsAsync()).ReturnsAsync(new List<Budget>());
            mockScheduleService.Setup(s => s.GetTasksForCurrentWeekAsync()).ReturnsAsync(new List<TaskItem>());
            mockToDoService.Setup(s => s.GetTodayTasksAsync()).ReturnsAsync(new List<ToDoItem>());
            mockToDoService.Setup(s => s.GetDailySchedulesAsync()).ReturnsAsync(new List<DailySchedule>());
            mockBillService.Setup(s => s.GetBillsForMonthAsync(It.IsAny<int>(), It.IsAny<int>()))
                           .ReturnsAsync(new List<Bill>());

            var controller = new BudgetController(
                mockBudgetService.Object,
                mockScheduleService.Object,
                mockToDoService.Object,
                mockTransactionService.Object,
                mockBillService.Object
            );

            // ACT
            var result = await controller.Index() as ViewResult;
            var model = result?.Model as BudgetWithTasksViewModel;

            // ASSERT
            Assert.NotNull(model);
            Assert.Empty(model.Budgets);
            Assert.Empty(model.CurrentWeekTasks);
            Assert.Empty(model.TodayTasks);
            Assert.Empty(model.DailySchedules);
            Assert.Empty(model.MonthlyBills);
        }

        /// <summary>
        /// Checks that attempting to edit a non-existent budget returns a <see cref="NotFoundResult"/>.
        /// </summary>
        [Fact]
        public async Task Edit_NonExistentBudget_ReturnsNotFound()
        {
            // ARRANGE
            var mockBudgetService = new Mock<IBudgetService>();
            mockBudgetService
                .Setup(svc => svc.GetBudgetDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync((Budget?)null);

            var controller = new BudgetController(
                mockBudgetService.Object,
                Mock.Of<IScheduleService>(),
                Mock.Of<IToDoService>(),
                Mock.Of<ITransactionService>(),
                Mock.Of<IBillService>()
            );

            // ACT
            var result = await controller.Edit(99);

            // ASSERT
            Assert.IsType<NotFoundResult>(result);
        }

        // End of Tests
    }
}
