using Dal.Converters;
using Dal.Models;
using Dal.Repositories.Interfaces;
using Dto;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dal.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PtachiyaContext _context;

        public PaymentRepository(PtachiyaContext context)
        {
            _context = context;
        }

        public async Task<List<PaymentDto>> GetAllAsync()
        {
            var entities = await _context.Payments.ToListAsync();
            // המרה מפורשת ל-PaymentDto כפי שמוגדר בממשק
            return entities.Select(e => PaymentConverter.ToPaymentDto(e)).ToList();
        }

        public async Task<Payment?> GetByIDAsync(int id)
        {
            return await _context.Payments.FindAsync(id);
        }

        public async Task AddAsync(Payment entity)
        {
            _context.Payments.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Payment entity)
        {
            _context.Payments.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment != null)
            {
                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Payment>> GetByChildIdAsync(int childId)
        {
            return await _context.Payments
                .Where(p => p.ChildId == childId)
                .ToListAsync();
        }
    }
}