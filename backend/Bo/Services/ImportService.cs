using Bo.Interfaces;
using Dal.Converters;
using Dal.Models;
using Dal.Repositories.Interfaces;
using Dal_Repository.ModelsConverters;
using Dto;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Bo.Services
{
    public class ImportService : IImportService
    {
        private readonly IChildRepository _childRepo;
        private readonly IKindergartenRepository _kindergartenRepo;

        public ImportService(
            IChildRepository childRepo,
            IKindergartenRepository kindergartenRepo)
        {
            _childRepo = childRepo;
            _kindergartenRepo = kindergartenRepo;
        }

        /// <summary>
        /// מייבא נתוני ילדים ומעדכן או יוצר לפי הצורך
        /// </summary>
        public async Task ImportChildDataAsync(List<ParentChildImportDto> data)
        {
            foreach (var row in data)
            {
                // בדיקה שהגן קיים לפי קוד
                var kindergarten = await _kindergartenRepo.GetByCodeAsync(row.KindergartenId);
                if (kindergarten == null)
                {
                    throw new KeyNotFoundException(
                        $"גן עם קוד {row.KindergartenId} לא נמצא עבור הילד {row.FullName}.");
                }

                int kindergartenId = kindergarten.KindergartenId;

                // טיפול בילד - עדכון או יצירה
                await UpsertChildAsync(row, kindergartenId);
            }
        }

        /// <summary>
        /// מייבא או מעדכן ילד יחיד על פי מספר זהות
        /// </summary>
        private async Task UpsertChildAsync(ParentChildImportDto row, int kindergartenId)
        {
            var existingChild = await _childRepo.GetByIdNumberAsync(row.IdNumber);

            if (row.BirthDate == null)
                throw new InvalidDataException($"תאריך לידה חסר עבור הילד {row.FullName}.");

            if (existingChild != null)
            {
                // עדכון ילד קיים
                existingChild.FullName = row.FullName;
                existingChild.BirthDate = row.BirthDate.Value;
                existingChild.SchoolYear = row.SchoolYear;
                existingChild.KindergartenId = kindergartenId;
                existingChild.FormLink = row.FormLink;
                existingChild.IdNumber=row.IdNumber;
                existingChild.Phone = row.Phone; // טלפון ההורה נכנס לשדה של הילד
                existingChild.Email = row.Email; // אימייל ההורה נכנס לשדה של הילד
                await _childRepo.UpdateAsync(existingChild);
            }
            else
            {
                // יצירת ילד חדש
                var newChild = ChildConverter.ToChildEntity(
                    row,
                    kindergartenId,
                    paymentId: 0, // אם אין מידע על תשלום כרגע
                    phone: row.Phone,
                    email: row.Email
                );

                await _childRepo.AddAsync(newChild);
            }
        }

        /// <summary>
        /// מייבא את כל הגנים למערכת
        /// </summary>
        public async Task ImportKindergartenDataAsync(List<KindergartenDto> data)
        {
            foreach (var dto in data)
            {
                var existing = await _kindergartenRepo.GetByCodeAsync(dto.Code);

                if (existing != null)
                {
                    // עדכון גן קיים
                    existing.Code = dto.Code;
                    existing.Name = dto.Name;

                    await _kindergartenRepo.UpdateAsync(existing);
                }
                else
                {
                    // יצירת גן חדש
                    var newEntity = KindergartenConverter.TokindergartenEntity(dto);
                    await _kindergartenRepo.AddAsync(newEntity);
                }
            }
        }
    }
}
 