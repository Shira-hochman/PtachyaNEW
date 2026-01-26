using Bo.Interfaces;
using Dal.Models;
using Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[EnableCors("AllowSpecificOrigin")]
public class FormController : ControllerBase
{
    private readonly IFormService _formService;
    private readonly IFileStorageService _fileStorageService;
    private const string AttachmentsFolder = "DiscountAttachments";

    public FormController(IFormService formService, IFileStorageService fileStorageService)
    {
        _formService = formService;
        _fileStorageService = fileStorageService;
    }

    // =================================================================
    // 🟢 אזור הורים - שליחה למייל ול-DB (בלי הורדה)
    // =================================================================

    [HttpPost("submit-health-declaration")]
    [Authorize(Roles = "Parent")]
    public async Task<IActionResult> SubmitHealthDeclaration([FromBody] HealthDeclarationDto declarationDto)
    {
        // אימות שההורה לא שולח עבור ילד אחר
        var authenticatedChildIdNumber = User.Claims.FirstOrDefault(c => c.Type == "ChildIdNumber")?.Value;
        if (authenticatedChildIdNumber != declarationDto.ChildDetails.ChildId.ToString())
        {
            return StatusCode(403, "אינך מורשה לשלוח טופס זה עבור ילד זה.");
        }

        try
        {
            // הסרביס מייצר PDF, שומר ב-DB ושולח מייל להורה
            await _formService.ProcessAndGenerateHealthDeclarationAsync(declarationDto);

            // מחזירים Ok בלבד - המייל כבר נשלח מה-Service
            return Ok(new { message = "הטופס נשלח בהצלחה למייל שלכם ונשמר במערכת." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"שגיאה בשליחת הטופס: {ex.Message}");
        }
    }
    [HttpPost("submit-discount-request")]
    [Authorize(Roles = "Parent")]
    public async Task<IActionResult> SubmitDiscountRequest([FromForm] DiscountRequestSubmissionDto formSubmission)
    {
        DiscountRequestDto requestDto;
        try
        {
            requestDto = JsonSerializer.Deserialize<DiscountRequestDto>(
                formSubmission.Data,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            )!;
        }
        catch (Exception) { return BadRequest("Failed to parse form data."); }

        var authenticatedChildIdNumber = User.Claims.FirstOrDefault(c => c.Type == "ChildIdNumber")?.Value;
        if (authenticatedChildIdNumber != requestDto.StudentDetails.StudentId.ToString())
        {
            return StatusCode(403, "אינך מורשה לשלוח טופס עבור תלמיד זה.");
        }

        try
        {
            // ⭐️ שינוי: ה-GetAttachments כעת מחזיר את כל הקבצים מכל הרשימות (הכנסה, חינוך מיוחד, עו"ס)
            // המתודה CombineAndSaveFiles כבר יודעת לרוץ בלולאה על הרשימה ולשמור את כולם.
            string allSavedPaths = await CombineAndSaveFiles(formSubmission.GetAttachments());

            await _formService.ProcessAndGenerateDiscountRequestAsync(requestDto, allSavedPaths);

            return Ok(new { message = "בקשת ההנחה נשלחה בהצלחה." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"שגיאה: {ex.Message}");
        }
    }

    // =================================================================
    // 🔵 אזור הנהלה - נשאר ללא שינוי כדי לא לפגוע בתהליך הקיים
    // =================================================================

    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPendingForms()
    {
        var formsEntities = await _formService.GetPendingFormsAsync();
        return Ok(formsEntities.Select(MapToDto).ToList());
    }

    [HttpGet("approved")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetApprovedForms()
    {
        var formsEntities = await _formService.GetApprovedFormsAsync();
        return Ok(formsEntities.Select(MapToDto).ToList());
    }

    [HttpPut("approve/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveForm(int id)
    {
        await _formService.ApproveFormAsync(id);
        return Ok(new { message = "הטופס אושר בהצלחה" });
    }

    [HttpGet("by-id-number/{idNumber}")]
    public async Task<IActionResult> GetFormsByIdNumber(string idNumber)
    {
        var forms = await _formService.GetFormsByIdNumberAsync(idNumber);
        return Ok(forms ?? new List<ChildFormDto>());
    }

    [HttpGet("Download")]
    public async Task<IActionResult> DownloadFile([FromQuery] string container, [FromQuery] string fileName)
    {
        // 🛡️ Security Fix
        fileName = Path.GetFileName(fileName);

        var allowedContainers = new[]
        {
        "DiscountAttachments",
        "PermanentForms"
    };

        if (!allowedContainers.Contains(container))
        {
            return BadRequest("Invalid container specified.");
        }

        // ⭐️ חזרה ל־bin (איפה שהקבצים באמת נמצאים)
        var root = AppDomain.CurrentDomain.BaseDirectory;
        var fullPath = Path.Combine(root, container, fileName);
        Console.WriteLine(fullPath);

        if (!System.IO.File.Exists(fullPath))
            return NotFound("הקובץ לא נמצא.");

        var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
        string contentType = fileName.EndsWith(".pdf")
            ? "application/pdf"
            : "application/octet-stream";

        return File(fileBytes, contentType, fileName);
    }


    // פונקציות עזר פנימיות
    private async Task<string> CombineAndSaveFiles(List<IFormFile> files)
    {
        var savedPaths = new List<string>();
        foreach (var file in files)
        {
            if (file.Length > 0) savedPaths.Add(await _fileStorageService.SaveFileAsync(file, AttachmentsFolder));
        }
        return string.Join(",", savedPaths);
    }

    private FormManageDto MapToDto(Dal.Models.Form f)
    {
        return new FormManageDto
        {
            FormId = f.FormId,
            FormType = f.FormType,
            Status = f.Status,
            SubmittedDate = f.SubmittedDate,
            FilePath = f.FilePath,
            ChildFirstName = f.Child?.FirstName ?? "לא ידוע",
            ChildLastName = f.Child?.lastName ?? "",
            ChildIdNumber = f.Child?.IdNumber ?? ""
        };
    }
}