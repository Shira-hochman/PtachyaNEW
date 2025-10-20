using Dal.Models;
using Dal.Repositories.Interfaces;
using Dal_Repository.ModelsConverters;
using Dto;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ptachya.DAL.Repositories
{
    public class ChildRepository : IChildRepository
    {
        private readonly PtachiyaContext _context;

        public ChildRepository(PtachiyaContext context)
        {
            _context = context;
        }

        public async Task<List<ChildDto>> GetAllAsync()
        {
            var entities = await _context.Children.ToListAsync();
            return entities.Select(ChildConverter.ToChildDto).ToList();
        }

        public async Task AddAsync(Child entity)
        {
            _context.Children.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<Child?> GetByIdNumberAsync(string idNumber)
        {
            return await _context.Set<Child>().FirstOrDefaultAsync(c => c.IdNumber == idNumber);
        }
        public async Task UpdateAsync(Child child)
        {
            _context.Children.Update(child);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int childId)
        {
            var entity = await _context.Children.FindAsync(childId);
            if (entity != null)
            {
                _context.Children.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}