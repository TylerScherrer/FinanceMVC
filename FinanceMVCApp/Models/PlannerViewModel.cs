using System;
using System.Collections.Generic;
using System.Linq;

namespace BudgetTracker.Models
{
    // ViewModel for managing tasks and schedules in a planner.
    // This class provides the necessary data and methods to manage daily schedules and tasks for a specific date or range of dates.
    public class PlannerViewModel
    {
        // The currently selected date for viewing or managing tasks.
        public DateTime SelectedDate { get; set; }

        // A list of all daily schedules associated with the planner.
        // Each DailySchedule represents a specific task assigned to a specific time slot.
        public List<DailySchedule> DailySchedules { get; set; }

        // A dictionary mapping dates to their corresponding daily schedules.
        // This allows efficient organization and retrieval of schedules for a specific date.
        public Dictionary<DateTime, List<DailySchedule>> TasksByDate { get; set; } = new Dictionary<DateTime, List<DailySchedule>>();

        // Retrieves a list of formatted date strings from the TasksByDate dictionary.
        // The dates are formatted as "yyyy-MM-dd" for consistency and easier display in the UI.
        // Returns: A list of date strings representing all the keys (dates) in TasksByDate.
        public List<string> GetTaskDates()
        {
            return TasksByDate.Keys.Select(date => date.ToString("yyyy-MM-dd")).ToList();
        }
    }
}
