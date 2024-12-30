using BudgetTracker.Models;
using Microsoft.EntityFrameworkCore;
using BudgetTracker.Data;


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
        public DbSet<DailySchedule> DailySchedules { get; set; }
        public DbSet<Bill> Bills { get; set; }


        public object BudgetCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>()
                .HasOne(c => c.Budget) // Category has one Budget
                .WithMany(b => b.Categories) // Budget has many Categories
                .HasForeignKey(c => c.BudgetId) // Foreign key is BudgetId
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete if Budget is removed

            // Configure the relationship between Bill and Budget
            modelBuilder.Entity<Bill>()
                .HasOne(b => b.Budget) // A Bill is associated with one Budget
                .WithMany(b => b.Bills) // A Budget can have many Bills
                .HasForeignKey(b => b.BudgetId) // Foreign key is BudgetId
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete if Budget is removed
        }
    }
}
