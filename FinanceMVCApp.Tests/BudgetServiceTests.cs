using BudgetTracker.Data;
using BudgetTracker.Models;
using BudgetTracker.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;


namespace BudgetTracker.Tests.Services
{
    public class BudgetServiceTests
    {
        /// <summary>
        /// Helper method to create a new ApplicationDbContext using InMemoryDatabase
        /// </summary>
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // ensures each test runs against its own database
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetAllBudgetsAsync_WhenCalled_ReturnsListOfBudgets()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new BudgetService(context);

            // Seed the database
            context.Budgets.Add(new Budget { Name = "Test Budget 1", TotalAmount = 1000 });
            context.Budgets.Add(new Budget { Name = "Test Budget 2", TotalAmount = 2000 });
            await context.SaveChangesAsync();

            // Act
            var budgets = await service.GetAllBudgetsAsync();

            // Assert
            Assert.NotNull(budgets);
            Assert.Equal(2, budgets.Count);
            Assert.Contains(budgets, b => b.Name == "Test Budget 1");
            Assert.Contains(budgets, b => b.Name == "Test Budget 2");
        }

        [Fact]
        public async Task GetBudgetDetailsAsync_ValidId_ReturnsBudgetDetails()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new BudgetService(context);

            var budget = new Budget { Name = "Details Test", TotalAmount = 500 };
            context.Budgets.Add(budget);
            await context.SaveChangesAsync();

            // Act
            var fetchedBudget = await service.GetBudgetDetailsAsync(budget.Id);

            // Assert
            Assert.NotNull(fetchedBudget);
            Assert.Equal(budget.Id, fetchedBudget.Id);
            Assert.Equal("Details Test", fetchedBudget.Name);
        }

        [Fact]
        public async Task GetBudgetDetailsAsync_InvalidId_ThrowsInvalidOperationException()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new BudgetService(context);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.GetBudgetDetailsAsync(9999)); // non-existing ID
        }

        [Fact]
        public async Task CreateBudgetAsync_ValidBudget_CreatesAndReturnsBudget()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new BudgetService(context);

            var newBudget = new Budget
            {
                Name = "New Budget",
                TotalAmount = 1500
            };

            // Act
            var createdBudget = await service.CreateBudgetAsync(newBudget);

            // Assert
            Assert.NotNull(createdBudget);
            Assert.True(createdBudget.Id > 0); // ID should be assigned
            Assert.Equal("New Budget", createdBudget.Name);

            var budgetInDb = await context.Budgets.FindAsync(createdBudget.Id);
            Assert.NotNull(budgetInDb);
            Assert.Equal(1500, budgetInDb.TotalAmount);
        }

        [Fact]
        public async Task CreateBudgetAsync_NullBudget_ThrowsArgumentNullException()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new BudgetService(context);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.CreateBudgetAsync(null!));
        }

        [Fact]
        public async Task CreateBudgetAsync_NegativeTotalAmount_ThrowsArgumentException()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new BudgetService(context);

            var invalidBudget = new Budget
            {
                Name = "Invalid Budget",
                TotalAmount = -100
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateBudgetAsync(invalidBudget));
        }

        [Fact]
        public async Task UpdateBudgetAsync_ValidBudget_UpdatesAndReturnsUpdatedBudget()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new BudgetService(context);

            var existingBudget = new Budget
            {
                Name = "Original Name",
                TotalAmount = 700
            };
            context.Budgets.Add(existingBudget);
            await context.SaveChangesAsync();

            // Modify the budget
            existingBudget.Name = "Updated Name";
            existingBudget.TotalAmount = 999;

            // Act
            var updatedBudget = await service.UpdateBudgetAsync(existingBudget);

            // Assert
            Assert.Equal("Updated Name", updatedBudget.Name);
            Assert.Equal(999, updatedBudget.TotalAmount);

            var budgetInDb = await context.Budgets.FindAsync(existingBudget.Id);
            Assert.Equal("Updated Name", budgetInDb.Name);
            Assert.Equal(999, budgetInDb.TotalAmount);
        }

        [Fact]
        public async Task UpdateBudgetAsync_NotFoundBudget_ThrowsInvalidOperationException()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new BudgetService(context);

            var nonExistingBudget = new Budget
            {
                Id = 9999, // Not in the DB
                Name = "Does not exist",
                TotalAmount = 1234
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.UpdateBudgetAsync(nonExistingBudget));
        }

        [Fact]
        public async Task DeleteBudgetAsync_ValidId_RemovesBudgetAndReturnsTrue()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new BudgetService(context);

            var budget = new Budget { Name = "Delete Test", TotalAmount = 300 };
            context.Budgets.Add(budget);
            await context.SaveChangesAsync();

            // Act
            var result = await service.DeleteBudgetAsync(budget.Id);

            // Assert
            Assert.True(result);
            Assert.Null(await context.Budgets.FindAsync(budget.Id));
        }

        [Fact]
        public async Task DeleteBudgetAsync_InvalidId_ReturnsFalse()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var service = new BudgetService(context);

            // Act
            var result = await service.DeleteBudgetAsync(9999);

            // Assert
            Assert.False(result);
        }
    }
}
