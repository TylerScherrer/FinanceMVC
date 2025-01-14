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
    }
}
