//using Bo.Interfaces;
//using Dto;
//using Microsoft.AspNetCore.Mvc;

//namespace Ptachya.API.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class ParentController : ControllerBase
//    {
//        private readonly IParentService _service;

//        public ParentController(IParentService service)
//        {
//            _service = service;
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetAll()
//        {
//            var parents = await _service.GetParentsAsync();
//            return Ok(parents);
//        }

//        [HttpPost]
//        public async Task<IActionResult> Add(ParentDto dto)
//        {
//            await _service.AddParentAsync(dto);
//            return Ok("Parent added successfully");
//        }
//    }
//}
