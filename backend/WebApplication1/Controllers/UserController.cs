using Bo.Interfaces;
using Dal.Converters;
using Dal.Models;
using Dto;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowSpecificOrigin")] // ⬅️ הוסף את האטריביוט הזה ל-Controller
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;

        public UserController(IUserService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var user = await _service.GetUserAsync();
            return Ok(user);
        }
        


        [HttpPost]
        public async Task<IActionResult> Add(UserDto dto)
        {
            await _service.AddUserAsync(dto);
            return Ok("User added successfully");
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserDto dto)
        {
            // הפונקציה ValidateUserWithFeedbackAsync מחזירה: 
            // - אובייקט User (בהצלחה)
            // - מחרוזת "לא קיים" (שם משתמש לא נמצא)
            // - מחרוזת "סיסמה שגויה" (סיסמה לא תואמת)
            var validationResult = await _service.ValidateUserWithFeedbackAsync(dto.Username, dto.PasswordHash);

            // בודק האם התוצאה היא אובייקט מסוג User (כלומר, הצלחה)
            if (validationResult is User user)
            {
                return Ok(new
                {
                    isSuccess = true,
                    message = $"שלום {user.Username}!",
                    // ניתן להוסיף פרטים נוספים של המשתמש אם רצוי
                    userId = user.UserId
                });
            }

            // אם התוצאה היא מחרוזת, זהו כשל
            if (validationResult is string errorMessage)
            {
                if (errorMessage == "סיסמה שגויה")
                {
                    return Unauthorized(new // 401 Unauthorized
                    {
                        isSuccess = false,
                        message = "סיסמה שגויה" // שם משתמש קיים, סיסמה שגויה
                    });
                }
                else if (errorMessage == "לא קיים")
                {
                    return Unauthorized(new // 401 Unauthorized
                    {
                        isSuccess = false,
                        message = "לא קיים" // שם משתמש לא קיים
                    });
                }
                // טיפול במקרה של שגיאה לא צפויה אחרת 
                return BadRequest(new { isSuccess = false, message = "שגיאת אימות" });
            }

            // טיפול במקרה שלא מתקבל אף אחד מהסוגים הצפויים
            return BadRequest(new { isSuccess = false, message = "שגיאה פנימית" });
        }


    }
}
