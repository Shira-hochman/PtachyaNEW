using Dal.Models;
using Dal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dal.Repositories
{
    // שימוש בשם KindergartenRepository במקום שם כפול
    public class KindergartenRepository : IKindergartenRepository
    {
        private readonly PtachiyaContext _context;


        public KindergartenRepository(PtachiyaContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Kindergarten>> GetAllAsync()
        {
            return await _context.Set<Kindergarten>()
                                 .ToListAsync();
        }

        public async Task<Kindergarten?> GetByIdAsync(int id)
        {
            return await _context.Set<Kindergarten>()
                                 .FirstOrDefaultAsync(k => k.KindergartenId == id);
        }

        public async Task AddAsync(Kindergarten kindergarten)
        {
            await _context.Set<Kindergarten>().AddAsync(kindergarten);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Kindergarten kindergarten)
        {
            _context.Set<Kindergarten>().Update(kindergarten);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _context.Set<Kindergarten>().Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<Kindergarten?> GetByCodeAsync(string code)
        {
            return await _context.Set<Kindergarten>()
                                 .FirstOrDefaultAsync(k => k.Code == code);
        }

    }
}