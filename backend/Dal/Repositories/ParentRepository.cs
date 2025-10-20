//using Dal.Converters;
//using Dal.Models;
//using Dal.Repositories.Interfaces;
//using Dto;
//using Microsoft.EntityFrameworkCore;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Dal.Repositories
//{
//    public class ParentRepository : IParentRepository
//    {
//        private readonly PtchiyaContext _context;

//        public ParentRepository(PtchiyaContext context)
//        {
//            _context = context;
//        }

//        public async Task<List<ParentDto>> GetAllAsync()
//        {
//            var entities = await _context.Parents.ToListAsync();
//            return entities.Select(ParentConverter.ToParentDto).ToList();
//        }

//        // *** תיקון: חתימה תואמת לממשק: מקבל Entity (Parent) ***
//        public async Task AddAsync(Parent entity)
//        {
//            // אין צורך בהמרה, אנו מקבלים Entity מוכן
//            _context.Parents.Add(entity);
//            await _context.SaveChangesAsync();
//        }

//        public async Task<Parent?> GetByEmailAsync(string email)
//        {
//            return await _context.Set<Parent>().FirstOrDefaultAsync(k => k.Email == email);
//        }

//        // ... (UpdateAsync ו-DeleteAsync)
//        public async Task UpdateAsync(Parent parent)
//        {
//            _context.Parents.Update(parent);
//            await _context.SaveChangesAsync();
//        }

//        public async Task DeleteAsync(int parentId)
//        {
//            var entity = await _context.Parents.FindAsync(parentId);
//            if (entity != null)
//            {
//                _context.Parents.Remove(entity);
//                await _context.SaveChangesAsync();
//            }
//        }
//    }
//}