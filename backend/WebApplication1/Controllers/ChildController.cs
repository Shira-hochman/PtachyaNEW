using Bo.Interfaces;
using Bo.Services;
using Dto;
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

        public ChildController(IChildService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var children = await _service.GetChildrenAsync();
            return Ok(children);
        }

        [HttpPost]
        public async Task<IActionResult> Add(ChildDto dto)
        {
            await _service.AddChildAsync(dto);
            return Ok("Child added successfully");
        }
        [HttpPost("verify")] // שינוי ל-HttpPost
                             // קבלת הנתונים מגוף הבקשה (FromQuery -> FromBody)
        public async Task<IActionResult> VerifyIdentity([FromBody] VerificationRequest request)
        {
            // משתמשים באובייקט כדי לקרוא את הנתונים
            if (string.IsNullOrEmpty(request.IdNumber) || request.BirthDate == default(DateTime))
            {
                return BadRequest("יש לספק מספר תעודת זהות ותאריך לידה.");
            }

            string result = await _service.VerifyChildIdentityAsync(request.IdNumber, request.BirthDate);

            // ניתן להחזיר קוד סטטוס מתאים יותר בהתאם לתוצאה, אך כאן אנו עוקבים אחר הפלט המבוקש
            return Ok(result);
        }

        // ⚠️ דרוש קלאס חדש עבור ה-request body
        public class VerificationRequest
        {
            public string IdNumber { get; set; }
            public DateTime BirthDate { get; set; }
        }

    }
}
