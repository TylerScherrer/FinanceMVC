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
              // Configure Bill-Budget relationship
            modelBuilder.Entity<Bill>()
                .HasOne(b => b.Budget) // Bill has one Budget
                .WithMany(b => b.Bills) // Budget has many Bills
                .HasForeignKey(b => b.BudgetId) // Foreign key
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete if Budget is removed
        }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    if (!optionsBuilder.IsConfigured)
    {
        optionsBuilder.UseMySql(
           "Server=fitnessserver.mysql.database.azure.com;Port=3306;Database=budget;Uid=TylerS00;Pwd=Blink182!;SslMode=Required;",
            new MySqlServerVersion(new Version(8, 0, 28)),
            options => options.EnableRetryOnFailure()
        );
    }
}
    }
    
}
