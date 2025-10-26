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
            
            var validationResult = await _service.ValidateUserWithFeedbackAsync(dto.Username, dto.PasswordHash);

            if (validationResult is User user)
            {
                return Ok(new
                {
                    isSuccess = true,
                    message = $"שלום {user.Username}!",
                    userId = user.UserId
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


    }
}
