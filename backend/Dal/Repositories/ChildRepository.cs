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

        // בתוך ChildRepository.cs
        public async Task<Child?> GetByIdAsync(int id)
        {
            return await _context.Children.FindAsync(id);
        }

        public async Task<Child?> GetByIdNumberAsync(string idNumber)
        {
            return await _context.Children
                .Include(c => c.Kindergarten) // ⭐️ חשוב: טוען את נתוני הגן יחד עם הילד
                .FirstOrDefaultAsync(c => c.IdNumber == idNumber);
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
     int? kindergartenId,
     string? schoolYear) // <-- הוספת הפרמטר
        {
            IQueryable<Child> query = _context.Children
                .Include(c => c.Kindergarten)
                .Include(c => c.Forms);

            // 🔍 חיפוש חופשי (קיים אצלך)
            if (!string.IsNullOrWhiteSpace(searchTerm)) { /* ... הלוגיקה הקיימת שלך ... */ }

            // 🏫 סינון לפי גן (קיים אצלך)
            if (kindergartenId.HasValue)
            {
                query = query.Where(c => c.KindergartenId == kindergartenId.Value);
            }

            // 📅 סינון חדש: לפי שנה עברית
            if (!string.IsNullOrWhiteSpace(schoolYear))
            {
                query = query.Where(c => c.SchoolYear == schoolYear);
            }

            var total = await query.CountAsync();
            var entities = await query
                .OrderBy(c => c.ChildId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<ChildDto>
            {
                Items = entities.Select(c => {
                    var dto = ChildConverter.ToChildDto(c);
                    dto.HasApprovedDiscount = c.Forms.Any(f => f.FormType == "DISCOUNT_REQUEST" && f.Status == "Approved");
                    return dto;
                }).ToList(),
                TotalCount = total
            };
        }

    }
}