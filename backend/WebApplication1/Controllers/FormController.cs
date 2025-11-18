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
    private const string AttachmentsFolder = "DiscountAttachments"; // ⭐️ תיקייה לקבצים מצורפים

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
    // ⭐️⭐️⭐️ נקודת הקצה המעודכנת: מקבלת FromForm DTO ⭐️⭐️⭐️
    [HttpPost("submit-discount-request")]
    public async Task<IActionResult> SubmitDiscountRequest([FromForm] DiscountRequestSubmissionDto formSubmission) // ⭐️ השתמש ב-DTO החדש
    {
        // 1. פיענוח נתוני הטופס מתוך ה-JSON string
        DiscountRequestDto requestDto;
        try
        {
            // PropertyNameCaseInsensitive = true נחוץ כי האנגולר שולח camelCase
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

        // 2. בדיקת הרשאה (כפי שהייתה קודם)
        var authenticatedChildIdNumber = User.Claims.FirstOrDefault(c => c.Type == "ChildIdNumber")?.Value;
        if (authenticatedChildIdNumber != requestDto.StudentDetails.StudentId.ToString())
        {
            return StatusCode(403, "אינך מורשה לשלוח טופס הנחה עבור תלמיד זה.");
        }

        // 3. שמירת הקבצים המצורפים פיזית על הדיסק ואיסוף נתיביהם
        string allSavedPaths = await SaveAndCombineUploadedFiles(formSubmission.GetAttachments()); // ⭐️ שמירה ואיסוף נתיבים (CSV)

        try
        {
            // 4. קורא לשירות החדש המטפל בבקשת ההנחה, כולל נתיבי הקבצים שנשמרו
            // ⭐️ חתימת השירות ProcessAndGenerateDiscountRequestAsync תצטרך להשתנות!
            byte[] fileBytes = await _formService.ProcessAndGenerateDiscountRequestAsync(requestDto, allSavedPaths);

            // 5. יצירת שם קובץ PDF והחזרתו
            string newFileName = $"Discount_Request_{requestDto.StudentDetails.StudentId}_{DateTime.Now:yyyyMMdd}.pdf";
            return File(fileBytes, "application/pdf", newFileName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Controller Error (Discount): {ex.Message}");
            return StatusCode(500, $"שגיאה פנימית בשרת בעת יצירת קובץ ההנחה: {ex.Message}");
        }
    }

    // ⭐️⭐️⭐️ פונקציית עזר לשמירת קבצים והפיכתם ל-CSV ⭐️⭐️⭐️
    private async Task<string> SaveAndCombineUploadedFiles(List<IFormFile> files)
    {
        var savedFileNames = new List<string>();
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var attachmentsDirectory = Path.Combine(baseDirectory, AttachmentsFolder);
        Directory.CreateDirectory(attachmentsDirectory);

        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                // יצירת שם קובץ ייחודי (GUID) + שם הקובץ המקורי
                string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                string filePath = Path.Combine(attachmentsDirectory, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                // שומרים את שם הקובץ הייחודי (שם בלבד)
                savedFileNames.Add(uniqueFileName);
            }
        }

        // מחזירים את כל שמות הקבצים המופרדים בפסיק (CSV)
        return string.Join(",", savedFileNames);
    }
}