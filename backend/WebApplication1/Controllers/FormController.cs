using Dto;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using Bo.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Linq; // חובה עבור Linq
using Microsoft.AspNetCore.Http; // ⭐️ הוסף
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
[EnableCors("AllowSpecificOrigin")]
[Authorize(Roles = "Parent")] // ⭐️ הגנה כללית על הקונטרולר להורה
public class FormController : ControllerBase
{
    private readonly IFormService _formService;
    private readonly IFileStorageService _fileStorageService; // ⭐️ הוספה: הזרקת השירות החדש
    private const string AttachmentsFolder = "DiscountAttachments";

    public FormController(IFormService formService, IFileStorageService fileStorageService) // ⭐️ הוספה לקונסטרוקטור
    {
        _formService = formService;
        _fileStorageService = fileStorageService;
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
    [HttpPost("submit-discount-request")]
    public async Task<IActionResult> SubmitDiscountRequest([FromForm] DiscountRequestSubmissionDto formSubmission)
    {
        // 1. פיענוח נתונים (נשאר כפי שהוא)
        DiscountRequestDto requestDto;
        // ... (קוד פיענוח ו-BadRequest) ...
        try
        {
            requestDto = JsonSerializer.Deserialize<DiscountRequestDto>(
                formSubmission.Data,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            )!;
            if (requestDto == null) return BadRequest("Invalid form data structure.");
        }
        catch (Exception)
        {
            return BadRequest("Failed to parse form data.");
        }

        // 2. בדיקת הרשאה (נשאר כפי שהוא)
        var authenticatedChildIdNumber = User.Claims.FirstOrDefault(c => c.Type == "ChildIdNumber")?.Value;
        if (authenticatedChildIdNumber != requestDto.StudentDetails.StudentId.ToString())
        {
            return StatusCode(403, "אינך מורשה לשלוח טופס הנחה עבור תלמיד זה.");
        }

        // 3. שמירת הקבצים המצורפים
        string allSavedPaths = await CombineAndSaveFiles(formSubmission.GetAttachments());

        // ⭐️⭐️⭐️ הבלוק הראשי עם ה-try/catch המשוקם ⭐️⭐️⭐️
        try
        {
            // 4. קורא לשירות החדש המטפל בבקשת ההנחה
            byte[] fileBytes = await _formService.ProcessAndGenerateDiscountRequestAsync(requestDto, allSavedPaths);

            // 5. יצירת שם קובץ PDF והחזרתו
            string newFileName = $"Discount_Request_{requestDto.StudentDetails.StudentId}_{DateTime.Now:yyyyMMdd}.pdf";
            return File(fileBytes, "application/pdf", newFileName); // ✅ החזרה מוצלחת
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Controller Error (Discount): {ex.Message}");
            // ✅ החזרת שגיאה (500) במקרה של כשל
            return StatusCode(500, $"שגיאה פנימית בשרת בעת יצירת קובץ ההנחה: {ex.Message}");
        }
        // 🛑 אין צורך ב-return נוסף כאן
    }
    private async Task<string> CombineAndSaveFiles(List<IFormFile> files)
    {
        var savedPaths = new List<string>();
        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                // שימוש בשירות ה-Interface החדש
                // שימו לב: השדה AttachmentsFolder הוגדר כקבוע במחלקה
                string path = await _fileStorageService.SaveFileAsync(file, AttachmentsFolder);
                savedPaths.Add(path);
            }
        }
        // מחזירים את כל שמות הקבצים המופרדים בפסיק (CSV)
        return string.Join(",", savedPaths);
    }
}
