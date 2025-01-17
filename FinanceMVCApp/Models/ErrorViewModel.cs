namespace BudgetTracker.Models;

// ViewModel used to represent error details in the application.
// This class provides information that can be displayed to the user or logged for debugging purposes.
// It ensures that errors are handled gracefully, allowing the application to display meaningful error messages.
public class ErrorViewModel
{
    // A unique identifier for the request where the error occurred.
    // This can be useful for tracing and debugging specific issues in server logs or diagnostics tools.
    public string? RequestId { get; set; }

    // A computed property that determines if the RequestId should be displayed.
    // If the RequestId is not null or empty, this returns true.
    // This is particularly useful for displaying the RequestId on error pages for easier debugging.
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
