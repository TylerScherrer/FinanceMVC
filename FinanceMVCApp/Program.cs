using BudgetTracker.Data;
using BudgetTracker.Interfaces;
using BudgetTracker.Services;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Register scoped services
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IToDoService, ToDoService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IBillService, BillService>();

// Configure MySQL database context with retry logic
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 27)), // Replace with your MySQL version
        mysqlOptions => mysqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5, // Retry up to 5 times
            maxRetryDelay: TimeSpan.FromSeconds(10), // Wait up to 10 seconds between retries
            errorNumbersToAdd: null // Add MySQL error codes if needed
        )
    )
);

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
