using Bo.Interfaces;
using Dal.Converters;
using Dal.Models;
using Dal.Repositories.Interfaces;
using Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bo.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        public UserService(IUserRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<UserDto>> GetUserAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task AddUserAsync(UserDto dto)
        {
            var entity = UserConverter.ToUserEntity(dto); // שימוש בממיר
            await _repo.AddAsync(entity); // שולח Entity
        }
        public async Task<object> ValidateUserWithFeedbackAsync(string username, string password)
        {
            // 1. שלוף את המשתמש באמצעות ה-Repository
            var userEntity = await _repo.GetByUsernameAsync(username);

            // 2. אם המשתמש לא נמצא, החזר הודעת "לא קיים"
            if (userEntity == null)
            {
                return "לא קיים";
            }

            // 3. בצע השוואה מאובטחת של הסיסמה
            // ⚠️ יש להחליף את זה בקוד ה-Verification האמיתי שלך (למשל, _hasher.VerifyPassword).
            // כרגע נשתמש בהשוואה ישירה כדוגמה, אבל *אין* לעשות זאת בקוד אמיתי!
            bool isPasswordValid = userEntity.PasswordHash == password; // דוגמה בלבד

            // 4. אם הסיסמה לא נכונה, החזר הודעת "סיסמה שגויה"
            if (!isPasswordValid)
            {
                return "סיסמה שגויה";
            }

            // 5. הצלחה - החזר את אובייקט המשתמש
            return userEntity;
        }

    }
}