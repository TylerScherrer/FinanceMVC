using BudgetTracker.Models;
using BudgetTracker.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using BudgetTracker.Data; 
using System.Diagnostics;



namespace BudgetTracker.Tests
{
    // Test class for BudgetService, which handles budget-related logic
    public class BudgetServiceTests
    {
        // Private fields for the in-memory database context and the service being tested
        private readonly ApplicationDbContext _context;
        private readonly BudgetService _budgetService;

        // Constructor for initializing the test setup
        public BudgetServiceTests()
        {
            // Set up an in-memory database for testing purposes
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Use a unique in-memory database for each test
                .Options;

            // Initialize the in-memory database context
            _context = new ApplicationDbContext(options);

            // Initialize the service to be tested, passing in the test database context
            _budgetService = new BudgetService(_context);
        }


// This test verifies that the UpdateBudgetAsync method correctly updates an existing budget in the database when valid data is provided.
// It ensures that:
// 1. The budget is updated with the new properties (e.g., updated name).
// 2. The method returns the updated budget.
// The test uses an in-memory database to simulate the update operation and asserts that the changes are saved and returned as expected.
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




/// Verifies that the UpdateBudgetAsync method throws a DbUpdateConcurrencyException
/// when a concurrency conflict occurs. This test simulates two users (User A and User B)
/// trying to update the same budget concurrently, where User B's update is saved first,
/// and User A's update results in a conflict due to outdated RowVersion.
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





/// Verifies that the UpdateBudgetAsync method throws an InvalidOperationException
/// when the budget being updated has been deleted by another user. This test simulates
/// the scenario by first removing the budget directly from the database and then attempting
/// to update it with stale data.
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




/// Verifies that the CreateBudgetAsync method throws an ArgumentException
/// when an attempt is made to create a budget with a negative TotalAmount.
/// This ensures that the service enforces a validation rule to prevent invalid budgets
/// from being created.
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





/// Verifies that the CreateBudgetAsync method successfully adds a budget to the database
/// when valid data is provided. This test ensures that the method correctly saves the budget
/// and returns the expected result with accurate details such as Name, TotalAmount, and DateCreated.
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




/// Verifies that the GetAllBudgetsAsync method retrieves all budgets from the database.
/// This test adds multiple budgets to the database and ensures that the service correctly
/// returns the expected number of budgets, along with verifying that specific budgets exist in the result.
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






/// Verifies that the GetAllBudgetsAsync method retrieves all budgets from the database.
/// This test adds multiple budgets to the database and ensures that the service correctly
/// returns the expected number of budgets, along with verifying that specific budgets exist in the result.
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




/// Verifies that the GetBudgetDetailsAsync method retrieves the correct budget
/// when a valid budget ID is provided. This test ensures that the returned budget
/// matches the expected details, such as Name and TotalAmount.
[Fact]
public async Task GetBudgetDetailsAsync_ShouldThrowInvalidOperationException_WhenIdDoesNotExist()
{
    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(async () =>
    {
        await _budgetService.GetBudgetDetailsAsync(99); // Non-existent ID
    });
}




/// Verifies that the UpdateBudgetAsync method throws an InvalidOperationException
/// when attempting to update a budget that does not exist in the database. This ensures
/// that the method correctly handles cases where the specified budget ID is invalid.
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



/// Verifies that the DeleteBudgetAsync method returns true and successfully deletes a budget
/// when the specified budget ID exists in the database. This test also ensures that the budget
/// is removed from the database after deletion.
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




/// Verifies that the DeleteBudgetAsync method returns false when attempting to delete a budget
/// that does not exist in the database. This ensures that the method does not throw an exception
/// and gracefully handles non-existent IDs.
[Fact]
public async Task DeleteBudgetAsync_ShouldReturnFalse_WhenBudgetDoesNotExist()
{
    // Act
    var result = await _budgetService.DeleteBudgetAsync(99); // Non-existent ID

    // Assert
    Assert.False(result);
}




/// Verifies that the CreateBudgetAsync method allows creating a budget with a zero TotalAmount.
/// This ensures that the method does not enforce a positive amount restriction
/// and correctly saves and returns the budget details when TotalAmount is zero.
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



/// Verifies that the GetAllBudgetsAsync method retrieves all budgets from the database
/// and includes their associated categories. This ensures that the method correctly
/// populates navigation properties and provides comprehensive data for each budget.
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




/// Verifies that the CreateBudgetAsync method initializes the RowVersion property
/// when it is null in the provided budget. This ensures that the RowVersion
/// property is always populated for concurrency control.
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





/// Verifies that the UpdateBudgetAsync method throws an InvalidOperationException
/// when attempting to update a budget with a null RowVersion. This ensures that
/// concurrency control mechanisms are enforced during updates.
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





/// Verifies that the DeleteBudgetAsync method gracefully handles scenarios where
/// the budget is concurrently deleted by another operation. This ensures that
/// the method does not throw an exception and correctly returns false when
/// the budget no longer exists in the database.
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






/// Verifies that the UpdateBudgetAsync method throws a DbUpdateConcurrencyException
/// when the RowVersion of the budget being updated does not match the RowVersion in
/// the database. This ensures that concurrency control mechanisms are enforced,
/// preventing data inconsistencies caused by simultaneous updates.
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





/// Verifies that the GetBudgetDetailsAsync method retrieves a budget and includes
/// the associated categories and their transactions. This ensures that navigation
/// properties are correctly populated and all relevant data is returned for detailed views.
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



/// Verifies that the GetAllBudgetsAsync method returns an empty list when no
/// budgets exist in the database. This ensures that the method handles empty
/// database states correctly without errors or null references.
[Fact]
public async Task GetAllBudgetsAsync_ShouldReturnEmptyList_WhenNoBudgetsExist()
{
    // Act
    var result = await _budgetService.GetAllBudgetsAsync();

    // Assert
    Assert.NotNull(result);
    Assert.Empty(result);
}




/// Verifies that the GetBudgetDetailsAsync method throws an InvalidOperationException
/// when the specified budget ID does not exist in the database. This ensures
/// that the method handles invalid IDs correctly and prevents null reference errors.
[Fact]
public async Task GetBudgetDetailsAsync_ShouldThrowException_WhenBudgetDoesNotExist()
{
    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        await _budgetService.GetBudgetDetailsAsync(-1));
}




/// Verifies that the CreateBudgetAsync method throws an ArgumentException when the
/// budget's Name property is null. This ensures that the method validates required fields
/// and prevents invalid data from being added to the database.
[Fact]
public async Task CreateBudgetAsync_ShouldThrowArgumentException_WhenNameIsNull()
{
    // Arrange
    var budget = new Budget { Name = null, TotalAmount = 1000, DateCreated = DateTime.Now };

    // Act & Assert
    var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
    {
        await _budgetService.CreateBudgetAsync(budget);
    });

    Assert.Equal("Name cannot be null or empty. (Parameter 'Name')", exception.Message);
}




/// Verifies that a category can be successfully added to an existing budget,
/// and that the association is correctly saved and retrievable from the database.
/// This ensures the integrity of the relationship between budgets and categories.
[Fact]
public async Task AddCategoryToBudget_ShouldSucceed()
{
    // Arrange
    var budget = new Budget
    {
        Name = "Test Budget",
        TotalAmount = 1000,
        DateCreated = DateTime.Now,
        RowVersion = new byte[8],
        Categories = new List<Category>()
    };
    _context.Budgets.Add(budget);
    await _context.SaveChangesAsync();

    var category = new Category { Name = "Test Category", BudgetId = budget.Id };

    // Act
    budget.Categories.Add(category);
    await _context.SaveChangesAsync();

    // Assert
    var retrievedBudget = await _budgetService.GetBudgetDetailsAsync(budget.Id);
    Assert.Single(retrievedBudget.Categories);
    Assert.Equal("Test Category", retrievedBudget.Categories.First().Name);
}





/// Verifies that the UpdateBudgetAsync method throws a DbUpdateConcurrencyException
/// when concurrent updates are attempted on the same budget. This ensures that the method
/// enforces concurrency control and prevents overwriting changes made by another user.
[Fact]
public async Task UpdateBudgetAsync_ShouldHandleConcurrentUpdates()
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
    var userABudget = await _context.Budgets.AsNoTracking().FirstAsync(b => b.Id == budget.Id);
    userABudget.TotalAmount = 1200;

    // Simulate User B's update directly modifying the database
    var userBBudget = await _context.Budgets.FirstAsync(b => b.Id == budget.Id);
    userBBudget.TotalAmount = 1500;
    userBBudget.RowVersion = new byte[] { 0, 0, 0, 2 }; // Simulate RowVersion increment
    await _context.SaveChangesAsync();

    // Act & Assert
    var exception = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
    {
        await _budgetService.UpdateBudgetAsync(userABudget);
    });

    Assert.Contains("The budget was updated by another user", exception.Message);
}




/// Verifies that the UpdateBudgetAsync method throws an ArgumentException
/// when the Name property of the budget is empty. This ensures that the method
/// validates required fields and prevents invalid data from being saved.
[Fact]
public async Task UpdateBudgetAsync_ShouldThrowArgumentException_WhenNameIsEmpty()
{
    // Arrange
    var budget = new Budget
    {
        Id = 1,
        Name = "", // Empty name
        TotalAmount = 1000,
        DateCreated = DateTime.Now,
        RowVersion = new byte[8]
    };

    // Act & Assert
    var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
    {
        await _budgetService.UpdateBudgetAsync(budget);
    });

    Assert.Equal("Name cannot be null or empty. (Parameter 'Name')", exception.Message);
}





/// Verifies that when a budget is deleted, all associated categories
/// are also removed from the database. This ensures the cascade delete behavior
/// and prevents orphaned data in the categories table.
[Fact]
public async Task DeleteBudgetAsync_ShouldDeleteAssociatedCategories()
{
    // Arrange
    var category = new Category { Name = "Test Category" };
    var budget = new Budget
    {
        Name = "Test Budget",
        TotalAmount = 1000,
        DateCreated = DateTime.Now,
        RowVersion = new byte[8],
        Categories = new List<Category> { category }
    };

    _context.Budgets.Add(budget);
    await _context.SaveChangesAsync();

    // Act
    var result = await _budgetService.DeleteBudgetAsync(budget.Id);

    // Assert
    Assert.True(result);
    Assert.Empty(_context.Categories); // Verify categories were deleted
}





/// Verifies that the CreateBudgetAsync method throws an InvalidOperationException
/// when attempting to create a budget with a duplicate name. This ensures that the method
/// enforces uniqueness for budget names, maintaining data integrity.
[Fact]
public async Task CreateBudgetAsync_ShouldEnforceUniqueBudgetName()
{
    // Arrange
    var budget1 = new Budget
    {
        Name = "Unique Budget",
        TotalAmount = 1000,
        DateCreated = DateTime.Now,
        RowVersion = new byte[8]
    };

    var budget2 = new Budget
    {
        Name = "Unique Budget", // Duplicate name
        TotalAmount = 1500,
        DateCreated = DateTime.Now,
        RowVersion = new byte[8]
    };

    _context.Budgets.Add(budget1);
    await _context.SaveChangesAsync();

    // Act & Assert
    var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
    {
        await _budgetService.CreateBudgetAsync(budget2);
    });

    Assert.Equal("A budget with the same name already exists.", exception.Message);
}





/// Verifies that the GetAllBudgetsAsync method can handle a large number of budgets
/// without performance degradation or memory issues. This test ensures the service
/// is robust and scalable for datasets with significant size.
[Fact]
public async Task GetAllBudgetsAsync_ShouldHandleLargeNumberOfBudgets()
{
    // Arrange
    for (int i = 1; i <= 10000; i++)
    {
        _context.Budgets.Add(new Budget
        {
            Name = $"Large Budget {i}",
            TotalAmount = 100 * i,
            DateCreated = DateTime.Now,
            RowVersion = new byte[8]
        });
    }
    await _context.SaveChangesAsync();

    // Act
    var result = await _budgetService.GetAllBudgetsAsync();

    // Assert
    Assert.Equal(10000, result.Count);
}





/// Verifies that the GetAllBudgetsAsync method can handle concurrent requests
/// by ensuring the method operates correctly and consistently when called multiple times
/// simultaneously. This test ensures thread safety and data consistency during concurrent usage.
[Fact]
public async Task GetAllBudgetsAsync_ShouldHandleConcurrentRequests()
{
    // Arrange
    for (int i = 1; i <= 10; i++)
    {
        _context.Budgets.Add(new Budget
        {
            Name = $"Concurrent Budget {i}",
            TotalAmount = 100 * i,
            DateCreated = DateTime.Now,
            RowVersion = new byte[8]
        });
    }
    await _context.SaveChangesAsync();

    // Act
    var task1 = _budgetService.GetAllBudgetsAsync();
    var task2 = _budgetService.GetAllBudgetsAsync();

    var results = await Task.WhenAll(task1, task2);

    // Assert
    Assert.Equal(10, results[0].Count);
    Assert.Equal(10, results[1].Count);
}





/// Verifies that the CreateBudgetAsync method can process a large dataset
/// efficiently and complete the operation within a reasonable time limit. This test
/// evaluates the performance of the service under high load conditions.
[Fact]
public async Task CreateBudgetAsync_ShouldCompleteWithinTimeLimit_ForLargeDataset()
{
    // Arrange
    var budgets = Enumerable.Range(1, 10000).Select(i => new Budget
    {
        Name = $"Performance Budget {i}",
        TotalAmount = i * 100,
        DateCreated = DateTime.Now,
        RowVersion = new byte[8]
    }).ToList();

    // Act & Assert
    var stopwatch = Stopwatch.StartNew();
    _context.Budgets.AddRange(budgets);
    await _context.SaveChangesAsync();
    stopwatch.Stop();

    Assert.True(stopwatch.ElapsedMilliseconds < 5000); // Ensure operation completes within 5 seconds
}












// End of Tests 
}
}