using Bo.Interfaces;
using Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using System.Threading.Tasks;
using Dal.Repositories.Interfaces;

[ApiController]
[Route("api/[controller]")]
[EnableCors("AllowSpecificOrigin")]
public class DashboardController : ControllerBase
{
    private readonly IFormRepository _formRepo; // נשתמש ברפוזיטורי ישירות לצורך פשטות הסטטיסטיקה

    public DashboardController(IFormRepository formRepo)
    {
        _formRepo = formRepo;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _formRepo.GetDashboardStatsAsync();
        return Ok(stats);
    }
}