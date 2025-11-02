using Dto;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Bo.Interfaces;
[ApiController]
[Route("api/[controller]")]
[EnableCors("AllowSpecificOrigin")] // ⬅️ הוסף את האטריביוט הזה ל-Controller
public class FormController : ControllerBase
{
    private readonly IFormService _formService;

    public FormController(IFormService formService)
    {
        _formService = formService;
    }
    [HttpPost("submit-health-declaration")]
    public async Task<IActionResult> SubmitHealthDeclaration([FromBody] HealthDeclarationDto declarationDto)
    {
        try
        {
            // 1. קריאה לשירות: השירות מחזיר כעת רק byte[] של PDF.
            // ⚠️ שינוי חתימה: יש לוודא ש-IFormService וה-FormService הותאמו להחזיר רק Task<byte[]>
            byte[] fileBytes = await _formService.ProcessAndGenerateHealthDeclarationAsync(declarationDto);

            // 2. יצירת שם קובץ PDF נקי חדש להורדה בדפדפן
            string newFileName = $"Health_Declaration_{declarationDto.ChildDetails.ChildId}.pdf";

            // 3. החזרת הקובץ ל-Angular כ-PDF תקין
            return File(fileBytes, "application/pdf", newFileName);
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