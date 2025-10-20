using Bo.Interfaces;
using Dto;
using Microsoft.AspNetCore.Mvc;
using Bo.Services;

namespace Ptachya.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
    }
}
