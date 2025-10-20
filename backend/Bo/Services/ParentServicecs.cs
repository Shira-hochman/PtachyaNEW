//using Bo.Interfaces;
//using Dal.Models;
//using Dal.Repositories.Interfaces;
//using Dto;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace Bo.Services
//{
//    public class ParentService : IParentService
//    {
//        private readonly IParentRepository _repo;

//        public ParentService(IParentRepository repo)
//        {
//            _repo = repo;
//        }

//        public async Task<List<ParentDto>> GetParentsAsync()
//        {
//            return await _repo.GetAllAsync();
//        }

//        // *** תיקון CS1503: המרת DTO ל-Entity לפני הקריאה לרפוזיטורי ***
//        public async Task AddParentAsync(ParentDto dto)
//        {
//            // דורש מתודת המרה מ-ParentDto ל-Parent Entity
//            var entity = new Parent // יצירה ידנית או שימוש ב-Converter
//            {
//                FullName = dto.FullName,
//                Phone = dto.Phone,
//                Email = dto.Email
//            };

//            await _repo.AddAsync(entity); // שולח Entity
//        }

//        public async Task RemoveParentAsync(ParentDto dto)
//        {
//            if (dto.ParentId <= 0)
//            {
//                throw new ArgumentException("Parent ID must be valid for deletion.");
//            }
//            await _repo.DeleteAsync(dto.ParentId);
//        }
//    }
//}