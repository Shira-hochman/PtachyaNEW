using Bo.Interfaces;
using Dal.Converters;
using Dal.Models;
using Dal.Repositories.Interfaces;
using Dto;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Bo.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly IPasswordHasher<User> _passwordHasher; // ✅ חדש

        public UserService(IUserRepository repo, IPasswordHasher<User> passwordHasher) // ✅ חדש
        {
            _repo = repo;
            _passwordHasher = passwordHasher; // ✅ חדש
        }

        public async Task<List<UserDto>> GetUserAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task AddUserAsync(UserDto dto)
        {
            var entity = UserConverter.ToUserEntity(dto);

            // **מנגנון אבטחה 1: גיבוב הסיסמה**
            // ה-PasswordHash ב-DTO הוא למעשה הסיסמה בטקסט רגיל, נגבב אותה.
            entity.PasswordHash = _passwordHasher.HashPassword(entity, entity.PasswordHash); // ✅ חדש ומאובטח

            await _repo.AddAsync(entity);
        }

        public async Task<object> ValidateUserWithFeedbackAsync(string username, string password)
        {
            var userEntity = await _repo.GetByUsernameAsync(username);

            if (userEntity == null)
            {
                return "לא קיים";
            }

            // **מנגנון אבטחה 2: אימות הסיסמה**
            // השוואה מאובטחת של הסיסמה שהוזנה מול ה-Hash השמור
            var verificationResult = _passwordHasher.VerifyHashedPassword(
                userEntity,
                userEntity.PasswordHash, // Hash השמור ב-DB
                password                 // הסיסמה שהוזנה ע"י המשתמש
            );

            // בדיקת התוצאה: Verified = הסיסמה נכונה, RehashNeeded = נכונה אבל צריך גיבוב מחדש
            bool isPasswordValid = verificationResult == PasswordVerificationResult.Success ||
                                   verificationResult == PasswordVerificationResult.SuccessRehashNeeded;

            if (!isPasswordValid)
            {
                return "סיסמה שגויה";
            }

            // ⚠️ במידה והסיסמה נכונה אבל נדרש גיבוב מחדש (למשל, שינוי באלגוריתם), כדאי לעדכן את ה-Hash
            // (לא חובה בשלב זה, אבל מומלץ)
            if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
            {
                // עדכון ה-Hash ב-DB ברקע
                userEntity.PasswordHash = _passwordHasher.HashPassword(userEntity, password);
                await _repo.UpdateAsync(userEntity);
            }

            return userEntity;
        }
    }

}
