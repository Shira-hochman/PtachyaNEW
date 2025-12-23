using Bo.Interfaces;
using Dto;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // ⭐️ ודא שאתה מייבא את זה

namespace Ptachya.API.Controllers
{
    // ⭐️ הוספת [Authorize] ו-[Authorize(Roles = "Admin")] ⭐️
    // זה מבטיח שרק משתמש מחובר ותפקידו הוא Admin יכול לגשת לבקר הזה.
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("AllowSpecificOrigin")]

    public class KindergartenController : ControllerBase
    {
        private readonly IKindergartenService _service;

        public KindergartenController(IKindergartenService service)
        {
            _service = service;
        }

        // דורש טוקן תקף ותפקיד Admin
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var kindergartens = await _service.GetKindergartensAsync();
            // הערה: יש לוודא ש-KindergartenDto הוא ה-DTO הנכון עבור הצד הלקוח.
            return Ok(kindergartens);
        }

        // דורש טוקן תקף ותפקיד Admin
        [HttpPost]
        public async Task<IActionResult> Add(KindergartenDto dto)
        {
            await _service.AddKindergartenAsync(dto);
            return Ok("Kindergarten added successfully");
        }
    }
}