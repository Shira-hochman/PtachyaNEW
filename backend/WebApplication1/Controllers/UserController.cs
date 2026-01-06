using Bo.Interfaces;
using Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowSpecificOrigin")]
    // 🔒 1. נעילה ראשית: כל הפעולות דורשות מנהל מחובר, אלא אם צוין אחרת
    [Authorize(Roles = "Admin")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;
        private readonly IConfiguration _config;

        public UserController(IUserService service, IConfiguration config)
        {
            _service = service;
            _config = config;
        }

        // שליפת כל המנהלים (לצורך הצגה בטבלה)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _service.GetUserAsync();

            // 🔒 2. אבטחה: מחזירים רק פרטים לא רגישים (בלי הסיסמה המוצפנת)
            var safeUsers = users.Select(u => new
            {
                u.UserId,
                u.Username
                // אפשר להוסיף כאן Email או שדות נוספים אם יש
            });

            return Ok(safeUsers);
        }

        // הוספת מנהל חדש (דורש טוקן של מנהל קיים)
        [HttpPost("register-admin")]
        public async Task<IActionResult> AddAdmin([FromBody] CreateAdminDto request)
        {
            // המרה מה-DTO של הבקשה ל-DTO של המערכת
            // אנחנו שמים את הסיסמה הגלויה בתוך PasswordHash באופן זמני,
            // כי הסרביס שלך (UserService) יודע לקחת את זה משם ולהצפין את זה.
            var userDto = new UserDto
            {
                Username = request.Username,
                PasswordHash = request.Password // הסיסמה הגלויה נשלחת להצפנה בסרביס
            };

            await _service.AddUserAsync(userDto);
            return Ok(new { message = "מנהל נוסף בהצלחה" });
        }

        // כניסה למערכת (פתוח לכולם - אחרת אי אפשר להיכנס)
        [AllowAnonymous] // 🔓 3. החרגה: כולם יכולים לגשת לפה
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserDto dto)
        {
            // שולחים לאימות (הסרביס בודק את הסיסמה מול ההצפנה)
            var validationResult = await _service.ValidateUserWithFeedbackAsync(dto.Username, dto.PasswordHash);

            // אם חזר אובייקט משתמש - ההתחברות הצליחה
            if (validationResult is Dal.Models.User user) // ודאי ש-User מגיע מה-Namespace הנכון
            {
                var token = GenerateJwtToken(user);

                return Ok(new
                {
                    isSuccess = true,
                    message = $"שלום {user.Username}!",
                    userId = user.UserId,
                    token = token,
                    role = "Admin"
                });
            }

            // טיפול בשגיאות התחברות
            if (validationResult is string errorMessage)
            {
                if (errorMessage == "סיסמה שגויה" || errorMessage == "לא קיים")
                {
                    return Unauthorized(new
                    {
                        isSuccess = false,
                        message = "שם משתמש או סיסמה שגויים" // עדיף הודעה כללית לאבטחה
                    });
                }
            }

            return BadRequest(new { isSuccess = false, message = "שגיאת התחברות כללית" });
        }

        // פונקציית עזר ליצירת הטוקן
        private string GenerateJwtToken(Dal.Models.User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // בדיקה שהטוקן תקין (לשימוש פנימי בקליינט)
        [HttpGet("check-auth")]
        public IActionResult CheckAuth()
        {
            return Ok(new { message = "Authenticated" });
        }

        // מחיקת מנהל
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdmin(int id)
        {
            await _service.DeleteUserAsync(id);
            return Ok(new { message = "המנהל נמחק בהצלחה" });
        }
    }


}

