using Bo.Interfaces;
using Dal.Models;
using Dal.Repositories.Interfaces;
using Dal_Repository.ModelsConverters;
using Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bo.Services
{
    public class ChildService : IChildService
    {
        private readonly IChildRepository _repo;

        public ChildService(IChildRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<ChildDto>> GetChildrenAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task AddChildAsync(ChildDto dto)
        {
            var entity = new Child
            {
                KindergartenId = dto.KindergartenId,
                IdNumber = dto.IdNumber,
                FullName = dto.FullName,
                SchoolYear = dto.SchoolYear,
                FormLink = dto.FormLink,
                Phone = dto.Phone,
                Email = dto.Email,
                BirthDate = dto.BirthDate,
            };

            await _repo.AddAsync(entity);
        }

        public async Task RemoveChildAsync(ChildDto dto)
        {
            if (dto.ChildId <= 0)
            {
                throw new ArgumentException("Child ID must be valid for deletion.");
            }
            await _repo.DeleteAsync(dto.ChildId);
        }

        // ❌ הוסר המתודה Private DateTime CorrectReversedDate
        // ה-Model Binder אמור לטפל בפורמט ISO של JSON כראוי

        public async Task<string> VerifyChildIdentityAsync(string idNumber, DateTime birthDate)
        {
            var childEntity = await _repo.GetByIdNumberAsync(idNumber);

            // 1. תעודת זהות שגויה (לא נמצאה רשומה)
            if (childEntity == null)
            {
                return "שגוי";
            }

            // 🛑 התיקון העדכני: השוואה מפורשת של היום, החודש והשנה
            // זה מבטיח שהזמן (Time) ואזור הזמן (Timezone) אינם משפיעים על האימות.
            bool isBirthDateMatch = (childEntity.BirthDate.Year == birthDate.Year) &&
                                    (childEntity.BirthDate.Month == birthDate.Month) &&
                                    (childEntity.BirthDate.Day == birthDate.Day);

            // 2. תעודת זהות נכונה, תאריך לידה שגוי
            if (!isBirthDateMatch)
            {
                return "אחד מהנתונים שהוקש שגוי";
            }

            // 3. הכל נכון
            return childEntity.FullName;
        }
    }
}
