// Bo.Services/ChildService.cs

using Bo.Interfaces;
using Dal.Models;
using Dal.Repositories.Interfaces;
using Dal_Repository.ModelsConverters; // הנחה: מחלקה זו ממירה מ-Child ל-ChildDto
using Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bo.Services
{
    // ⚠️ דורש הוספת המתודה ל-IChildService!
    // public interface IChildService { ... Task<ChildDto?> GetChildDetailsByIdAndBirthDateAsync(string idNumber, DateTime birthDate); }

    public class ChildService : IChildService
    {
        private readonly IChildRepository _repo;

        public ChildService(IChildRepository repo)
        {
            _repo = repo;
        }

        // ... מתודות GetChildrenAsync, AddChildAsync, RemoveChildAsync נשארות ללא שינוי ...

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
                //FullName = dto.FullName,
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

        // 🛑 מתודת האימות הישנה (מחזירה string)
        // אם היא עדיין נחוצה עבור קריאות ישנות, השאר אותה
        public async Task<string> VerifyChildIdentityAsync(string idNumber, DateTime birthDate)
        {
            var childEntity = await _repo.GetByIdNumberAsync(idNumber);

            if (childEntity == null) return "שגוי";

            bool isBirthDateMatch = (childEntity.BirthDate.Date == birthDate.Date);

            if (!isBirthDateMatch) return "אחד מהנתונים שהוקש שגוי";

            return childEntity.FirstName;
        }

        // ⭐️⭐️⭐️ מתודה חדשה: מאמתת ומחזירה את אובייקט ה-DTO המלא ⭐️⭐️⭐️
        public async Task<ChildDto?> GetChildDetailsByIdAndBirthDateAsync(string idNumber, DateTime birthDate)
        {
            // 1. שלוף את ישות הילד מה-DAL
            var childEntity = await _repo.GetByIdNumberAsync(idNumber);

            // אם הילד לא קיים, החזר null
            if (childEntity == null)
            {
                return null;
            }

            // 2. ודא שתאריך הלידה תואם (מבלי להתחשב בשעה/אזור זמן)
            bool isBirthDateMatch = (childEntity.BirthDate.Date == birthDate.Date);

            // אם התאריך לא תואם, החזר null (כדי שהקונטרולר יחזיר Unauthorized)
            if (!isBirthDateMatch)
            {
                return null;
            }

            // 3. אם האימות הצליח, המר את ישות ה-DAL ל-DTO והחזר אותה
            // הנחה: קיים ממיר (Converter) כלשהו (כמו ModelsConverters.ToDto)
            return ChildConverter.ToChildDto(childEntity);
        }
    }
}