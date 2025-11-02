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

        public ChildController(IChildService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize]
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
        [HttpPost("details")]
        public async Task<IActionResult> GetChildDetails([FromBody] VerificationRequest request)
        {
            if (string.IsNullOrEmpty(request.IdNumber) || request.BirthDate == default(DateTime))
            {
                return BadRequest("יש לספק מספר תעודת זהות ותאריך לידה.");
            }

            // ⭐️ שינוי: קוראים לפונקציה בשירות שמחזירה את כל אובייקט הילד (ChildDto)
            ChildDto? childDetails = await _service.GetChildDetailsByIdAndBirthDateAsync(request.IdNumber, request.BirthDate);

            if (childDetails == null)
            {
                // אם האימות נכשל (לא נמצא ילד), מחזירים 401 או 400
                return Unauthorized("מספר תעודת זהות או תאריך לידה שגויים.");
            }

            // ⭐️ מחזירים את אובייקט הילד המלא ב-JSON
            return Ok(childDetails);
        }

        // ⚠️ דרוש קלאס חדש עבור ה-request body
        public class VerificationRequest
        {
            public string IdNumber { get; set; }
            public DateTime BirthDate { get; set; }
        }

    }
}
