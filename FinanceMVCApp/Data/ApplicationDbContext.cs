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


    // *****************************
    // DbSet
    // *****************************
    // A DbSet represents a collection of entities of a specific type that can be queried from the database or written to it.
    // It acts as a bridge between the application and the database, enabling CRUD (Create, Read, Update, Delete) operations on the entities.
    // Each DbSet corresponds to a table in the database, and the generic type parameter <T> represents the entity type for that table.
    // For example, DbSet<Budget> corresponds to a "Budgets" table, holding Budget objects.
    // DbSet allows you to:
    // 1. Retrieve data: Query the database to fetch entities as objects.
    // 2. Add data: Insert new entities into the database.
    // 3. Update data: Modify existing entities and save changes to the database.
    // 4. Remove data: Delete entities from the database.
    // It provides an abstraction over database interactions, making it easier to work with data without writing raw SQL queries.
    // Represents the database table for storing Budget entities.
    // Each row in the Budgets table corresponds to a Budget object.
    public DbSet<Budget> Budgets { get; set; }

    // Represents the database table for storing Category entities.
    // Each row in the Categories table corresponds to a Category object.
    // Categories are linked to Budgets (e.g., a budget may have multiple categories).
    public DbSet<Category> Categories { get; set; }

    // Represents the database table for storing Transaction entities.
    // Each row in the Transactions table corresponds to a Transaction object.
    // Transactions are typically associated with Categories (e.g., a category may have multiple transactions).
    public DbSet<Transaction> Transactions { get; set; }

    // Represents the database table for storing TaskItem entities.
    // Each row in the Tasks table corresponds to a TaskItem object.
    // TaskItems might be used for tracking tasks within the application.
    public DbSet<TaskItem> Tasks { get; set; }

    // Represents the database table for storing ToDoItem entities.
    // Each row in the ToDoItems table corresponds to a ToDoItem object.
    // ToDoItems might be used for managing personal or project-based to-do lists.
    public DbSet<ToDoItem> ToDoItems { get; set; }

    // Represents the database table for storing DailySchedule entities.
    // Each row in the DailySchedules table corresponds to a DailySchedule object.
    // DailySchedules could be used to track daily activities or events.
    public DbSet<DailySchedule> DailySchedules { get; set; }

    // Represents the database table for storing Bill entities.
    // Each row in the Bills table corresponds to a Bill object.
    // Bills are linked to Budgets and might represent monthly or periodic expenses.
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
