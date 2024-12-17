using BudgetTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Budget> Budgets { get; set; } // DbSet for Budget
        public DbSet<Category> Categories { get; set; }

        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<ToDoItem> ToDoItems { get; set; }

        public object BudgetCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>()
                .HasOne(c => c.Budget) // Category has one Budget
                .WithMany(b => b.Categories) // Budget has many Categories
                .HasForeignKey(c => c.BudgetId) // Foreign key is BudgetId
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete if Budget is removed
        }

    }
}
