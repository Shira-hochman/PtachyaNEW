using Bo.Interfaces;
using Bo.Services;
using Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Ptachya.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("AllowSpecificOrigin")]
    public class ChildController : ControllerBase
    {
        private readonly IChildService _service;
        // ⭐️ 1. משתנה חדש לשירות התוקנים
        private readonly ITokenService _tokenService;

        // ⭐️ 2. הזרקת שירות התוקנים לבנאי (Constructor)
        public ChildController(IChildService service, ITokenService tokenService)
        {
            _service = service;
            _tokenService = tokenService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var children = await _service.GetChildrenAsync();
            return Ok(children);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add(ChildDto dto)
        {
            await _service.AddChildAsync(dto);
            return Ok("Child added successfully");
        }
        // ⭐️ 4. שינוי: נקודת הקצה (Endpoint) שמחזירה תוקן לאחר אימות
        [HttpPost("login")] // שיניתי את השם ל-login כי זו כניסה
        public async Task<IActionResult> Login([FromBody] VerificationRequest request)
        {
            if (string.IsNullOrEmpty(request.IdNumber) || request.BirthDate == default(DateTime))
            {
                return BadRequest("יש לספק מספר תעודת זהות ותאריך לידה.");
            }

            ChildDto? childDetails = await _service.GetChildDetailsByIdAndBirthDateAsync(request.IdNumber, request.BirthDate);

            if (childDetails == null)
            {
                return Unauthorized("מספר תעודת זהות או תאריך לידה שגויים.");
            }

            // ⭐️ 5. יצירת התוקן
            var token = _tokenService.GenerateToken(childDetails);

            // ⭐️ 6. החזרת התוקן יחד עם פרטי הילד
            return Ok(new
            {
                Token = token,
                Child = childDetails // נתוני הילד המלאים
            });
        }

        // בתוך ChildController.cs

        [HttpPut("{id}")]
        // [Authorize(Roles = "Admin")] // את יכולה להוריד את ההערה אם יש לך ניהול הרשאות
        public async Task<IActionResult> Update(int id, [FromBody] ChildDto dto)
        {
            if (id != dto.ChildId)
            {
                return BadRequest("מזהה הילד אינו תואם לנתונים שנשלחו.");
            }

            try
            {
                await _service.UpdateChildAsync(dto);
                return Ok(new { message = "פרטי הילד עודכנו בהצלחה" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound("הילד לא נמצא במערכת.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"שגיאה פנימית: {ex.Message}");
            }
        }
        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(
       [FromQuery] int page = 1,
       [FromQuery] int pageSize = 10,
       [FromQuery] string? searchTerm = null,
       [FromQuery] int? kindergartenId = null,
       [FromQuery] string? schoolYear = null) // <-- הוספת פרמטר
        {
            var result = await _service.GetChildrenPagedAsync(page, pageSize, searchTerm, kindergartenId, schoolYear);
            return Ok(result);
        }


    }


}
