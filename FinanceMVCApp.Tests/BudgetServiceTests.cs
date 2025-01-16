using BudgetTracker.Models;
using BudgetTracker.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using BudgetTracker.Data; // Adjust to the correct namespace.


namespace BudgetTracker.Tests
{
  public class BudgetServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly BudgetService _budgetService;

    public BudgetServiceTests()
    {
        // Use in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Ensure a unique database for each test
            .Options;

        _context = new ApplicationDbContext(options);
        _budgetService = new BudgetService(_context);
    }

[Fact]
public async Task UpdateBudgetAsync_ShouldUpdateBudget_WhenValid()
{
    // Arrange
    var budget = new Budget
    {
        Id = 1,
        Name = "Old Budget",
        TotalAmount = 1000,
        DateCreated = DateTime.Now,
        RowVersion = new byte[8] // Initialize RowVersion
    };
    _context.Budgets.Add(budget);
    await _context.SaveChangesAsync();

    budget.Name = "Updated Budget";

    // Act
    var result = await _budgetService.UpdateBudgetAsync(budget);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("Updated Budget", result.Name);
}


[Fact]
public async Task UpdateBudgetAsync_ShouldThrowDbUpdateConcurrencyException_WhenConcurrencyConflictOccurs()
{
    // Arrange
    var budget = new Budget
    {
        Id = 1,
        Name = "Original Budget",
        TotalAmount = 1000,
        DateCreated = DateTime.Now,
        RowVersion = new byte[] { 0, 0, 0, 1 } // Initial RowVersion
    };

    _context.Budgets.Add(budget);
    await _context.SaveChangesAsync();

    // Simulate User A's update
    var userABudget = new Budget
    {
        Id = budget.Id,
        Name = "User A's Budget",
        TotalAmount = 1200,
        DateCreated = budget.DateCreated,
        RowVersion = budget.RowVersion
    };

    // Simulate User B's update by directly modifying the database
    var userBBudget = await _context.Budgets.FirstAsync();
    userBBudget.Name = "User B's Budget";
    userBBudget.RowVersion = new byte[] { 0, 0, 0, 2 }; // Simulate RowVersion increment
    await _context.SaveChangesAsync();

    // Detach the User B's updated entity to simulate concurrency conflict
    _context.Entry(userBBudget).State = EntityState.Detached;

    // Act & Assert
    var exception = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
    {
        await _budgetService.UpdateBudgetAsync(userABudget);
    });

    Assert.Contains("The budget was updated by another user", exception.Message);
}





[Fact]
public async Task UpdateBudgetAsync_ShouldThrowInvalidOperationException_WhenBudgetIsDeleted()
{
    // Arrange
    var budget = new Budget
    {
        Id = 1,
        Name = "Test Budget",
        TotalAmount = 1000,
        DateCreated = DateTime.Now,
        RowVersion = new byte[8]
    };

    _context.Budgets.Add(budget);
    await _context.SaveChangesAsync();

    // Simulate User A's update
    var userABudget = new Budget
    {
        Id = budget.Id,
        Name = "Updated Budget",
        TotalAmount = 1200,
        DateCreated = budget.DateCreated,
        RowVersion = budget.RowVersion
    };

    // Simulate the budget being deleted
    var budgetToDelete = _context.Budgets.First();
    _context.Budgets.Remove(budgetToDelete);
    await _context.SaveChangesAsync();

    // Act & Assert
    var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
    {
        await _budgetService.UpdateBudgetAsync(userABudget);
    });

    Assert.Equal("Budget was deleted by another user.", exception.Message);
}




























































// End of Tests 
}
}