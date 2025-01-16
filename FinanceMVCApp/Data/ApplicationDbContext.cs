using BudgetTracker.Models;
using Microsoft.EntityFrameworkCore;
using BudgetTracker.Data;


namespace BudgetTracker.Data
{
    public class ApplicationDbContext : DbContext // Inheirts from the DbContext class, which allows it to gain the functionality of EF Core providing work for databases such as 
                                                  // querying, saving, and managing differnt changes 
    {

    // Constructor for ApplicationDbContext
    // ------------------------------------
    // This constructor is used to initialize an instance of the ApplicationDbContext class.
    // It takes a parameter of type DbContextOptions<ApplicationDbContext>, which contains all 
    // the configuration settings for the database context, such as the database provider, 
    // connection string, retry logic, and other EF Core-specific options.
    //
    // The generic type <ApplicationDbContext> ensures that these options are specifically 
    // for this context class, enabling EF Core to properly manage entities and database 
    // interactions for ApplicationDbContext.
    //
    // This constructor is registered with dependency injection in Program.cs, where EF Core 
    // is configured to use the desired database provider (e.g., MySQL) and any additional settings.
    //
    // The base(options) call:
    // - Passes the DbContextOptions<ApplicationDbContext> to the base DbContext class.
    // - Allows the DbContext class to be constructed and initialized with these options.
    // - Sets up the core functionality required to interact with the database, such as managing
    //   connections, executing queries, tracking changes, and more.
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

            modelBuilder.Entity<Budget>()
                .Property(b => b.RowVersion)
                .IsRowVersion()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

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
