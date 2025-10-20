using Bo.Interfaces;
using Dto;
using Microsoft.AspNetCore.Mvc;

namespace Ptachya.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KindergartenController : ControllerBase
    {
        private readonly IKindergartenService _service;

        public KindergartenController(IKindergartenService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var kindergartens = await _service.GetKindergartensAsync();
            return Ok(kindergartens);
        }

        [HttpPost]
        public async Task<IActionResult> Add(KindergartenDto dto)
        {
            await _service.AddKindergartenAsync(dto);
            return Ok("Kindergarten added successfully");
        }
    }
}
