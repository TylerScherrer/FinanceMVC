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




[Fact]
public async Task CreateBudgetAsync_ShouldThrowArgumentException_WhenTotalAmountIsNegative()
{
    // Arrange
    var budget = new Budget
    {
        Name = "Invalid Budget",
        TotalAmount = -500,
        DateCreated = DateTime.Now
    };

    // Act & Assert
    var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
    {
        await _budgetService.CreateBudgetAsync(budget);
    });

    Assert.Equal("TotalAmount cannot be negative. (Parameter 'TotalAmount')", exception.Message);
}


[Fact]
public async Task CreateBudgetAsync_ShouldAddBudget_WhenValid()
{
    // Arrange
    var budget = new Budget
    {
        Name = "Valid Budget",
        TotalAmount = 1000,
        DateCreated = DateTime.Now,
        RowVersion = new byte[] { 0, 0, 0, 1 } // Simulate concurrency support
    };

    // Act
    var result = await _budgetService.CreateBudgetAsync(budget);

    // Assert
    Assert.NotNull(result); // Ensure the budget is not null
    Assert.Equal("Valid Budget", result.Name);
    Assert.Equal(1000, result.TotalAmount);
    Assert.Equal(budget.DateCreated, result.DateCreated);
}


[Fact]
public async Task GetAllBudgetsAsync_ShouldReturnAllBudgets()
{
    // Arrange
    var budgets = new List<Budget>
    {
        new Budget { Id = 1, Name = "Budget 1", TotalAmount = 1000, DateCreated = DateTime.Now, RowVersion = new byte[8] },
        new Budget { Id = 2, Name = "Budget 2", TotalAmount = 2000, DateCreated = DateTime.Now, RowVersion = new byte[8] }
    };

    _context.Budgets.AddRange(budgets);
    await _context.SaveChangesAsync();

    // Act
    var result = await _budgetService.GetAllBudgetsAsync();

    // Assert
    Assert.Equal(2, result.Count);
    Assert.Contains(result, b => b.Name == "Budget 1");
    Assert.Contains(result, b => b.Name == "Budget 2");
}


[Fact]
public async Task GetBudgetDetailsAsync_ShouldReturnBudget_WhenIdExists()
{
    // Arrange
    var budget = new Budget
    {
        Id = 1,
        Name = "Existing Budget",
        TotalAmount = 1000,
        DateCreated = DateTime.Now,
        RowVersion = new byte[8]
    };

    _context.Budgets.Add(budget);
    await _context.SaveChangesAsync();

    // Act
    var result = await _budgetService.GetBudgetDetailsAsync(1);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("Existing Budget", result.Name);
}

[Fact]
public async Task GetBudgetDetailsAsync_ShouldThrowInvalidOperationException_WhenIdDoesNotExist()
{
    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(async () =>
    {
        await _budgetService.GetBudgetDetailsAsync(99); // Non-existent ID
    });
}


[Fact]
public async Task UpdateBudgetAsync_ShouldThrowInvalidOperationException_WhenBudgetDoesNotExist()
{
    // Arrange
    var budget = new Budget
    {
        Id = 99, // Non-existent ID
        Name = "Non-existent Budget",
        TotalAmount = 1000,
        DateCreated = DateTime.Now,
        RowVersion = new byte[8]
    };

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(async () =>
    {
        await _budgetService.UpdateBudgetAsync(budget);
    });
}

[Fact]
public async Task DeleteBudgetAsync_ShouldReturnTrue_WhenBudgetExists()
{
    // Arrange
    var budget = new Budget
    {
        Id = 1,
        Name = "Budget to Delete",
        TotalAmount = 1000,
        DateCreated = DateTime.Now,
        RowVersion = new byte[8]
    };

    _context.Budgets.Add(budget);
    await _context.SaveChangesAsync();

    // Act
    var result = await _budgetService.DeleteBudgetAsync(1);

    // Assert
    Assert.True(result);
    Assert.Empty(_context.Budgets); // Ensure the budget is removed
}

[Fact]
public async Task DeleteBudgetAsync_ShouldReturnFalse_WhenBudgetDoesNotExist()
{
    // Act
    var result = await _budgetService.DeleteBudgetAsync(99); // Non-existent ID

    // Assert
    Assert.False(result);
}



[Fact]
public async Task CreateBudgetAsync_ShouldAllowBudgetWithZeroTotalAmount()
{
    // Arrange
    var budget = new Budget
    {
        Name = "Zero Budget",
        TotalAmount = 0,
        DateCreated = DateTime.Now,
        RowVersion = new byte[8]
    };

    // Act
    var result = await _budgetService.CreateBudgetAsync(budget);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("Zero Budget", result.Name);
    Assert.Equal(0, result.TotalAmount);
}




[Fact]
public async Task GetAllBudgetsAsync_ShouldIncludeCategories()
{
    // Arrange
    var category = new Category
    {
        Id = 1,
        Name = "Category 1",
        BudgetId = 1 // Ensure the foreign key is set correctly
    };

    var budget = new Budget
    {
        Id = 1,
        Name = "Budget with Category",
        TotalAmount = 1000,
        DateCreated = DateTime.Now,
        RowVersion = new byte[8],
        Categories = new List<Category> { category } // Associate the category
    };

    // Add the budget and its category to the in-memory database
    _context.Budgets.Add(budget);
    await _context.SaveChangesAsync();

    // Act
    var result = await _budgetService.GetAllBudgetsAsync();

    // Assert
    Assert.Single(result); // Ensure one budget is returned
    Assert.Single(result[0].Categories); // Ensure one category is associated
    Assert.Equal("Category 1", result[0].Categories.First().Name); // Verify the category name
}





[Fact]
public async Task CreateBudgetAsync_ShouldInitializeRowVersion_WhenNull()
{
    // Arrange
    var budget = new Budget
    {
        Name = "Budget Without RowVersion",
        TotalAmount = 500,
        DateCreated = DateTime.Now,
        RowVersion = null
    };

    // Act
    var result = await _budgetService.CreateBudgetAsync(budget);

    // Assert
    Assert.NotNull(result.RowVersion);
    Assert.Equal(8, result.RowVersion.Length); // Default size for RowVersion
}



[Fact]
public async Task UpdateBudgetAsync_ShouldThrowInvalidOperationException_WhenRowVersionIsNull()
{
    // Arrange
    var budget = new Budget
    {
        Id = 1,
        Name = "Budget Without RowVersion",
        TotalAmount = 1000,
        DateCreated = DateTime.Now,
        RowVersion = null // Missing RowVersion
    };

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(async () =>
    {
        await _budgetService.UpdateBudgetAsync(budget);
    });
}

[Fact]
public async Task DeleteBudgetAsync_ShouldHandleConcurrentDeletion()
{
    // Arrange
    var budget = new Budget
    {
        Id = 1,
        Name = "Budget to Delete",
        TotalAmount = 1000,
        DateCreated = DateTime.Now,
        RowVersion = new byte[8]
    };

    _context.Budgets.Add(budget);
    await _context.SaveChangesAsync();

    // Simulate concurrent deletion
    var concurrentBudget = await _context.Budgets.FindAsync(1);
    _context.Budgets.Remove(concurrentBudget);
    await _context.SaveChangesAsync();

    // Act
    var result = await _budgetService.DeleteBudgetAsync(1);

    // Assert
    Assert.False(result); // Budget was already deleted
}

[Fact]
public async Task UpdateBudgetAsync_ShouldThrowDbUpdateConcurrencyException_WhenRowVersionMismatchOccurs()
{
    // Arrange
    var budget = new Budget
    {
        Id = 1,
        Name = "Original Budget",
        TotalAmount = 1000,
        DateCreated = DateTime.Now,
        RowVersion = new byte[] { 0, 0, 0, 1 }
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

    // Simulate User B's update
    var userBBudget = await _context.Budgets.FirstAsync();
    userBBudget.RowVersion = new byte[] { 0, 0, 0, 2 }; // Simulate RowVersion change
    await _context.SaveChangesAsync();

    // Detach tracked entities
    _context.Entry(budget).State = EntityState.Detached;
    _context.Entry(userBBudget).State = EntityState.Detached;

    // Act & Assert
    var exception = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
    {
        await _budgetService.UpdateBudgetAsync(userABudget);
    });

    Assert.Contains("The budget was updated by another user", exception.Message);
}

[Fact]
public async Task GetBudgetDetailsAsync_ShouldIncludeTransactionsInCategories()
{
    // Arrange
    var transaction = new Transaction
    {
        Id = 1,
        Description = "Transaction 1",
        Amount = 100
    };

    var category = new Category
    {
        Id = 1,
        Name = "Category 1",
        Transactions = new List<Transaction> { transaction }
    };

    var budget = new Budget
    {
        Id = 1,
        Name = "Test Budget",
        TotalAmount = 1000,
        Categories = new List<Category> { category },
        RowVersion = new byte[8]
    };

    _context.Budgets.Add(budget);
    await _context.SaveChangesAsync();

    // Act
    var result = await _budgetService.GetBudgetDetailsAsync(1);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("Test Budget", result.Name);
    Assert.Single(result.Categories);
    Assert.Single(result.Categories.First().Transactions);
    Assert.Equal("Transaction 1", result.Categories.First().Transactions.First().Description);
}












// End of Tests 
}
}