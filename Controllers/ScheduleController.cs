using Microsoft.AspNetCore.Mvc;
using BudgetTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using BudgetTracker.Data;
using BudgetTracker.Extensions;


namespace BudgetTracker.Controllers
{
    public class ScheduleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ScheduleController(ApplicationDbContext context)
        {
            _context = context;
        }
    public IActionResult Index()
    {
        var currentDate = DateTime.Now;

        // Get tasks for the current week
        var currentWeekTasks = _context.Tasks
            .Where(t => t.Date >= currentDate.StartOfWeek() && t.Date <= currentDate.EndOfWeek())
            .ToList();

        // Get tasks for the upcoming week
        var upcomingWeekTasks = _context.Tasks
            .Where(t => t.Date > currentDate.EndOfWeek() && t.Date <= currentDate.AddDays(14).EndOfWeek())
            .ToList();

        // Get tasks beyond the next two weeks
        var farthestTasks = _context.Tasks
            .Where(t => t.Date > currentDate.AddDays(14).EndOfWeek())
            .ToList();

        // Pass data to the view
        var model = new ScheduleViewModel
        {
            CurrentWeekTasks = currentWeekTasks,
            UpcomingWeekTasks = upcomingWeekTasks,
            FarthestTasks = farthestTasks // Fixed
        };

        return View(model);
    }


        [HttpPost]
        public IActionResult AddTask(string Name, DateTime Date)
        {
            if (!string.IsNullOrEmpty(Name) && Date != default)
            {
                var newTask = new TaskItem
                {
                    Name = Name,
                    Date = Date
                };

                _context.Tasks.Add(newTask);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }
    }
    }

