using BudgetTracker.Controllers;       // Matches 'namespace BudgetTracker.Controllers'
using BudgetTracker.Interfaces;        // If IBudgetService etc. are in BudgetTracker.Interfaces
using BudgetTracker.Models;           // If Budget, etc. are in BudgetTracker.Models
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetTracker.Tests // <-- or choose any consistent test namespace
{
    public class BudgetControllerTests
    {
      [Fact]
public async Task Index_ReturnsViewResult_WithViewModelContainingBudgets()
{
    // ARRANGE
    var mockBudgetService = new Mock<IBudgetService>();
    var mockScheduleService = new Mock<IScheduleService>();
    var mockToDoService = new Mock<IToDoService>();
    var mockTransactionService = new Mock<ITransactionService>();
    var mockBillService = new Mock<IBillService>();

    // Budgets mock
    mockBudgetService
        .Setup(svc => svc.GetAllBudgetsAsync())
        .ReturnsAsync(new List<Budget>
        {
            new Budget { Id = 1, Name = "Test Budget 1" }
        });

    // Bills mock (this fixes the ArgumentNullException)
    mockBillService
        .Setup(svc => svc.GetBillsForMonthAsync(It.IsAny<int>(), It.IsAny<int>()))
        .ReturnsAsync(new List<Bill>
        {
            new Bill { Id = 100, Name = "Test Bill", DueDate = DateTime.Now }
        });

    // Create the controller
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

    // Confirm the budgets
    Assert.Single(model.Budgets); // we set up exactly 1 budget above

    // Confirm we got the bills
    Assert.NotNull(model.MonthlyBills);
    Assert.Single(model.MonthlyBills);
}



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
        Name = "" // Invalid: Name is required
    };

    var controller = new BudgetController(
        mockBudgetService.Object,
        mockScheduleService.Object,
        mockToDoService.Object,
        mockTransactionService.Object,
        mockBillService.Object
    );

    controller.ModelState.AddModelError("Name", "The Name field is required.");

    // ACT
    var result = await controller.Create(invalidBudget);

    // ASSERT
    var viewResult = Assert.IsType<ViewResult>(result);
    Assert.Equal(invalidBudget, viewResult.Model);
}





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

    mockBillService
        .Setup(svc => svc.GetBillsForMonthAsync(It.IsAny<int>(), It.IsAny<int>()))
        .ReturnsAsync(new List<Bill>()); // No bills

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



[Fact]
public async Task Details_NullBudget_ReturnsNotFound()
{
    // ARRANGE
    var mockBudgetService = new Mock<IBudgetService>();
mockBudgetService
    .Setup(svc => svc.GetBudgetDetailsAsync(It.IsAny<int>()))
    .ReturnsAsync((Budget)null!); // Suppress nullability warning with null-forgiving operator


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

[Fact]
public async Task Edit_NullBudget_ReturnsNotFound()
{
    // ARRANGE
    var mockBudgetService = new Mock<IBudgetService>();
mockBudgetService
    .Setup(svc => svc.GetBudgetDetailsAsync(It.IsAny<int>()))
    .ReturnsAsync((Budget)null!); // Suppress nullability warning with null-forgiving operator


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

[Fact]
public async Task DeleteScheduledTask_ValidId_RedirectsToIndex()
{
    // ARRANGE
    var mockScheduleService = new Mock<IScheduleService>();
    mockScheduleService
        .Setup(svc => svc.DeleteTaskAsync(It.IsAny<int>()))
        .ReturnsAsync(true); // Return a Task<bool>

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










































































































































 // End of Tests

    }
}
