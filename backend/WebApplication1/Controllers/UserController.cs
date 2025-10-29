using Bo.Interfaces;
using Dal.Converters;
using Dal.Models;
using Dto;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt; // ✅ חדש
using System.Security.Claims;          // ✅ חדש
using Microsoft.IdentityModel.Tokens;  // ✅ חדש
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowSpecificOrigin")] // ⬅️ הוסף את האטריביוט הזה ל-Controller
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;
        private readonly IConfiguration _config; // ✅ הזרקת תצורה כדי לגשת למפתח הסודי

        public UserController(IUserService service, IConfiguration config) // ✅ הוספת config
        {
            _service = service;
            _config = config; // ✅
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
            var validationResult = await _service.ValidateUserWithFeedbackAsync(dto.Username, dto.PasswordHash);

            if (validationResult is User user)
            {
                // **מנגנון אבטחה 3: יצירת JWT**
                var token = GenerateJwtToken(user); // ✅ יצירת הטוקן

                return Ok(new
                {
                    isSuccess = true,
                    message = $"שלום {user.Username}!",
                    userId = user.UserId,
                    token = token // ✅ החזרת הטוקן ללקוח
                });
            }

            if (validationResult is string errorMessage)
            {
                if (errorMessage == "סיסמה שגויה")
                {
                    return Unauthorized(new 
                    {
                        isSuccess = false,
                        message = "סיסמה שגויה" 
                    });
                }
                else if (errorMessage == "לא קיים")
                {
                    return Unauthorized(new 
                    {
                        isSuccess = false,
                        message = "לא קיים" 
                    });
                }
                return BadRequest(new { isSuccess = false, message = "שגיאת אימות" });
            }

            return BadRequest(new { isSuccess = false, message = "שגיאה פנימית" });
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // יצירת הטענות (Claims) - המידע שאנו רוצים לשמור בטוקן
            var claims = new[]
            {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Role, "Admin") // ⚠️ נניח שכל משתמש מחובר הוא Admin
        };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(60), // ⚠️ הגדרת זמן תפוגה (מומלץ לקצר)
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ✅ הוספת מתודה מאובטחת לדוגמה (צריך להשתמש ב-Attribute [Authorize])
        [HttpGet("secure-test")]
        [Authorize] // ⬅️ רק משתמש עם טוקן תקין יכול לגשת
        public IActionResult SecureTest()
        {
            var username = User.Identity?.Name;
            return Ok($"ברוך הבא למסך המאובטח, {username}. הטוקן תקין.");
        }
    }

}

