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
    Assert.Equal("Bill 2", model.MonthlyBills[0].Name); // Bill with the earliest due date
    Assert.Equal("Bill 1", model.MonthlyBills[1].Name);
    Assert.Equal("Bill 3", model.MonthlyBills[2].Name);
}


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
    var viewResult = Assert.IsType<ViewResult>(result); // Now correctly returns ViewResult
    Assert.False(controller.ModelState.IsValid);
    Assert.Equal("Total amount must be greater than or equal to zero.",
                 controller.ModelState["TotalAmount"].Errors.FirstOrDefault()?.ErrorMessage);
}


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




























































































































 // End of Tests

    }
}
