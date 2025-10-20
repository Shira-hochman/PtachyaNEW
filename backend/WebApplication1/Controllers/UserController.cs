using Bo.Interfaces;
using Dal.Converters;
using Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
    
}
}
