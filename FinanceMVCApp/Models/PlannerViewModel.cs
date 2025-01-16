using System;
using System.Collections.Generic;
using System.Linq;

namespace BudgetTracker.Models
{
    public class PlannerViewModel
    {
        /// <summary>
        /// The currently selected date.
        /// </summary>
        public DateTime SelectedDate { get; set; }

        /// <summary>
        /// List of schedules for the currently selected day.
        /// </summary>
        public List<DailySchedule> DailySchedules { get; set; }

        /// <summary>
        /// Dictionary of tasks grouped by date.
        /// </summary>
        public Dictionary<DateTime, List<DailySchedule>> TasksByDate { get; set; } = new Dictionary<DateTime, List<DailySchedule>>();

        /// <summary>
        /// Converts the dictionary to a list of dates with tasks.
        /// </summary>
        public List<string> GetTaskDates()
        {
            return TasksByDate.Keys.Select(date => date.ToString("yyyy-MM-dd")).ToList();
        }
    }
}
