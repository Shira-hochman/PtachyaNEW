// FormController.cs

using Dto;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using Bo.Interfaces;

[ApiController]
[Route("api/[controller]")]
[EnableCors("AllowSpecificOrigin")]
public class FormController : ControllerBase
{
    private readonly IFormService _formService;

    public FormController(IFormService formService)
    {
        _formService = formService;
    }

    [HttpPost("submit-health-declaration")]
    // ⭐️ מחזירים Task<IActionResult> כי אנו מחזירים FileResult
    public async Task<IActionResult> SubmitHealthDeclaration([FromBody] HealthDeclarationDto declarationDto)
    {
        try
        {
            // 1. יצירת הקובץ ואיחזור מערך הבתים של ה-PDF
            byte[] fileBytes = await _formService.ProcessAndGenerateHealthDeclarationAsync(declarationDto);

            // 2. יצירת שם קובץ PDF נקי חדש להורדה
            string newFileName = $"Health_Declaration_{declarationDto.ChildDetails.ChildId}.pdf";

            // 3. החזרת הקובץ ל-Angular כ-PDF
            return File(fileBytes, "application/pdf", newFileName);
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Controller Error: {ex.Message}");
            return StatusCode(500, "שגיאה פנימית בעת יצירת הקובץ. אנא נסה שנית.");
        }
    }
}
