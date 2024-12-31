namespace BudgetTracker.Helpers
{
    public static class TimeFormatHelper
    {
        public static string FormatTimeSpanTo12Hour(TimeSpan time)
        {
            var dateTime = DateTime.Today.Add(time);
            return dateTime.ToString("hh:mm tt");
        }
    }
}
