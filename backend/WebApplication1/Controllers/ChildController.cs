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

        // ⚠️ דרוש קלאס חדש עבור ה-request body
        public class VerificationRequest
        {
            public string IdNumber { get; set; }
            public DateTime BirthDate { get; set; }
        }

    }
}
