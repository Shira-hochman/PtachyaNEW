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
            // הוסיפי Include גם כאן כדי למנוע בעיות בעתיד
            var entities = await _context.Children
                .Include(c => c.Kindergarten)
                .ToListAsync();

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
        // הוסף לממשק IChildRepository את החתימה:
        // Task<PagedResult<ChildDto>> GetPagedAsync(int page, int pageSize);

        // מימוש ב-ChildRepository:
        public async Task<PagedResult<ChildDto>> GetPagedAsync(
     int page,
     int pageSize,
     string? searchTerm,
     int? kindergartenId
 )
        {
            IQueryable<Child> query = _context.Children.Include(c => c.Kindergarten);
           

            // 🔍 חיפוש חופשי
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c =>
                    c.FirstName.Contains(searchTerm) ||
                    c.lastName.Contains(searchTerm) ||
                    c.IdNumber.Contains(searchTerm) ||
                    c.Email.Contains(searchTerm)
                );
            }

            // 🏫 סינון לפי גן
            if (kindergartenId.HasValue)
            {
                query = query.Where(c => c.KindergartenId == kindergartenId.Value);
            }

            var total = await query.CountAsync();

            var entities = await query
                .OrderBy(c => c.ChildId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<ChildDto>
            {
                Items = entities.Select(ChildConverter.ToChildDto).ToList(),
                TotalCount = total
            };
        }

    }
}