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
                BirthDate = dto.BirthDate, // הנחה שהתאריך נשמר נכון ב-DB
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

        private DateTime CorrectReversedDate(DateTime originalDate)
        {
            // נסה לתקן רק אם היום והחודש שונים, כדי להימנע משגיאות
            if (originalDate.Day != originalDate.Month)
            {
                try
                {
                    // יצירת תאריך חדש עם יום-חודש הפוכים
                    return new DateTime(originalDate.Year, originalDate.Day, originalDate.Month);
                }
                catch (ArgumentOutOfRangeException)
                {
                    // אם התאריך ההפוך אינו חוקי (כגון חודש 30), חזור למקורי
                    return originalDate;
                }
            }

            return originalDate;
        }

        public async Task<string> VerifyChildIdentityAsync(string idNumber, DateTime birthDate)
        {
            var childEntity = await _repo.GetByIdNumberAsync(idNumber);

            // 1. תעודת זהות שגויה (לא נמצאה רשומה)
            if (childEntity == null)
            {
                return "שגוי";
            }

            // תיקון הקלט שקיבלנו, בהנחה שהוא הפוך (MM/DD) ואילו הנתונים ב-DB נשמרו כראוי (DD/MM)
            DateTime correctedInputBirthDate = CorrectReversedDate(birthDate);

            // השוואה: נשווה בין התאריך השמור ב-DB לבין הקלט המתוקן. 
            // נשווה רק את התאריך (Day, Month, Year) ע"י שימוש ב-.Date
            bool isBirthDateMatch = childEntity.BirthDate.Date == correctedInputBirthDate.Date;

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