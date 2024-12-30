using BudgetTracker.Data;
using BudgetTracker.Interfaces;
using BudgetTracker.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetTracker.Services
{
    public class BillService : IBillService
    {
        private readonly ApplicationDbContext _context;

        public BillService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Bill> GetBillByIdAsync(int id)
        {
            return await _context.Bills.FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<List<Bill>> GetBillsAsync(int budgetId)
        {
            return await _context.Bills.Where(b => b.BudgetId == budgetId).ToListAsync();
        }
        public async Task CreateBillAsync(Bill bill)
        {
            await _context.Bills.AddAsync(bill);
            await _context.SaveChangesAsync();
        }



        public async Task<bool> DeleteBillAsync(int id)
        {
            var bill = await GetBillByIdAsync(id);
            if (bill == null)
                return false;

            _context.Bills.Remove(bill);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task UpdateBillAsync(Bill bill)
        {
            _context.Bills.Update(bill);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> MarkAsPaidAsync(int billId)
        {
            // Retrieve the bill by its ID
            var bill = await _context.Bills.FindAsync(billId);

            if (bill == null)
            {
                // Return false if the bill is not found
                return false;
            }

            // Update the IsPaid property
            bill.IsPaid = true;

            // Save the changes to the database
            await _context.SaveChangesAsync();

            return true; // Return true indicating success
        }
public async Task<List<Bill>> GetBillsForMonthAsync(int month, int year)
{
    return await _context.Bills
        .Where(b => b.DueDate.Month == month && b.DueDate.Year == year)
        .ToListAsync();
}

    }
}
