namespace BudgetTracker.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Sunday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        public static DateTime EndOfWeek(this DateTime date)
        {
            return StartOfWeek(date).AddDays(6);
        }
    }
}
