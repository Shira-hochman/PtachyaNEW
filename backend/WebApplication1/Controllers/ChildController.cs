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
        [HttpGet("verify")]
        public async Task<IActionResult> VerifyIdentity([FromQuery] string idNumber, [FromQuery] DateTime birthDate)
        {
            if (string.IsNullOrEmpty(idNumber) || birthDate == default(DateTime))
            {
                return BadRequest("יש לספק מספר תעודת זהות ותאריך לידה.");
            }

            string result = await _service.VerifyChildIdentityAsync(idNumber, birthDate);

            // ניתן להחזיר קוד סטטוס מתאים יותר בהתאם לתוצאה, אך כאן אנו עוקבים אחר הפלט המבוקש
            return Ok(result);
        }
    
}
}
