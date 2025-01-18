using BudgetTracker.Data; // Namespace for database context
using BudgetTracker.Interfaces; // Interfaces for dependency injection
using BudgetTracker.Services; // Services implementing the interfaces
using Microsoft.EntityFrameworkCore; // Entity Framework Core for database operations
using Pomelo.EntityFrameworkCore.MySql; // MySQL provider for EF Core

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews(); 
// Registers MVC services, enabling controllers and views for the application

// Register scoped services (Dependency Injection)
builder.Services.AddScoped<ICategoryService, CategoryService>(); // Category-related operations
builder.Services.AddScoped<IBudgetService, BudgetService>(); // Budget management services
builder.Services.AddScoped<IScheduleService, ScheduleService>(); // Schedule-related logic
builder.Services.AddScoped<IToDoService, ToDoService>(); // To-Do task management
builder.Services.AddScoped<ITransactionService, TransactionService>(); // Transaction-related services
builder.Services.AddScoped<IBillService, BillService>(); // Bill management

// Configure MySQL database context with retry logic
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"), // Connection string from configuration
        new MySqlServerVersion(new Version(8, 0, 27)), // Specify the version of the MySQL server being used
        mysqlOptions => mysqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5, // Retry up to 5 times for transient failures
            maxRetryDelay: TimeSpan.FromSeconds(10), // Maximum delay between retries
            errorNumbersToAdd: null // Optionally specify MySQL error codes to trigger retries
        )
    )
);

var app = builder.Build(); // Builds the application

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Use a custom error page in production
    app.UseHsts(); // Enforce HTTP Strict Transport Security (HSTS) in production
}

app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS
app.UseStaticFiles(); // Enable serving of static files (CSS, JS, etc.)

app.UseRouting(); // Enables routing to controllers and endpoints
app.UseAuthorization(); // Enables authorization middleware

// Define the default routing pattern for controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
// Default route: directs requests to HomeController's Index action, with optional id

app.Run(); // Runs the application
