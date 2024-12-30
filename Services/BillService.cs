using BudgetTracker.Data;
using BudgetTracker.Interfaces;
using BudgetTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Services
{
    public class BillService : IBillService
    {
        private readonly ApplicationDbContext _context;

        public BillService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Bill>> GetBillsAsync(int budgetId)
        {
            return await _context.Bills
                .Where(b => b.BudgetId == budgetId)
                .ToListAsync();
        }

        public async Task<Bill> CreateBillAsync(Bill bill)
        {
            _context.Bills.Add(bill);
            await _context.SaveChangesAsync();
            return bill;
        }

        public async Task<bool> DeleteBillAsync(int id)
        {
            var bill = await _context.Bills.FindAsync(id);
            if (bill == null) return false;

            _context.Bills.Remove(bill);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
