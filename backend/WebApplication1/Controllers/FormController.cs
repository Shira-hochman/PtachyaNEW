using Dto;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using Bo.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Linq; // חובה עבור Linq

[ApiController]
[Route("api/[controller]")]
[EnableCors("AllowSpecificOrigin")]
[Authorize(Roles = "Parent")] // ⭐️ הגנה כללית על הקונטרולר להורה
public class FormController : ControllerBase
{
    private readonly IFormService _formService;

    public FormController(IFormService formService)
    {
        _formService = formService;
    }

    [HttpPost("submit-health-declaration")]
    // ⭐️ שיטת הצהרת בריאות קיימת
    public async Task<IActionResult> SubmitHealthDeclaration([FromBody] HealthDeclarationDto declarationDto)
    {
        // בדיקת הרשאה: ודא שהמשתמש המאומת שולח טופס רק עבור הילד שלו
        var authenticatedChildIdNumber = User.Claims.FirstOrDefault(c => c.Type == "ChildIdNumber")?.Value;

        if (authenticatedChildIdNumber != declarationDto.ChildDetails.ChildId.ToString())
        {
            // 🛑 תיקון: שימוש ב-403 עם הודעה מותאמת אישית
            return StatusCode(403, "אינך מורשה לשלוח טופס זה עבור ילד זה. (403)");
        }
        try
        {
            byte[] fileBytes = await _formService.ProcessAndGenerateHealthDeclarationAsync(declarationDto);
            string newFileName = $"Health_Declaration_{declarationDto.ChildDetails.ChildId}.pdf";
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

    // ⭐️⭐️⭐️ נקודת קצה חדשה לטופס בקשת הנחה ⭐️⭐️⭐️
    [HttpPost("submit-discount-request")]
    public async Task<IActionResult> SubmitDiscountRequest([FromBody] DiscountRequestDto requestDto)
    {
        // ⭐️ בדיקת הרשאה: ודא שהת"ז של הילד המבוקש תואמת לת"ז המאומתת בתוקן
        var authenticatedChildIdNumber = User.Claims.FirstOrDefault(c => c.Type == "ChildIdNumber")?.Value;

        // נשתמש ב-StudentId כהתאמה לילד המאומת (נניח שזה הילד הראשי)
        if (authenticatedChildIdNumber != requestDto.StudentDetails.StudentId)
        {
            // 🛑 תיקון: שימוש ב-403 עם הודעה מותאמת אישית
            return StatusCode(403, "אינך מורשה לשלוח טופס הנחה עבור תלמיד זה.");
        }

        try
        {
            // קורא לשירות החדש המטפל בבקשת ההנחה
            byte[] fileBytes = await _formService.ProcessAndGenerateDiscountRequestAsync(requestDto);

            // יצירת שם קובץ PDF
            string newFileName = $"Discount_Request_{requestDto.StudentDetails.StudentId}_{DateTime.Now:yyyyMMdd}.pdf";

            return File(fileBytes, "application/pdf", newFileName);
        }
        catch (FileNotFoundException ex)
        {
            // שגיאה זו כנראה נבעה מכשל ב-Python
            Console.WriteLine($"Controller Error (FileNotFound): {ex.Message}");
            return StatusCode(500, "שגיאת שרת: קובץ PDF לא נוצר או תבנית חסרה.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Controller Error (Discount): {ex.Message}");
            // שגיאה זו כנראה הגיעה מה-Python (Exit Code != 0)
            return StatusCode(500, $"שגיאה פנימית בשרת בעת יצירת קובץ ההנחה: {ex.Message}");
        }
    }
}