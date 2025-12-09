using Dto;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using Bo.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Collections.Generic;

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
    // 🟢 אזור הורים (שליחת טפסים) - מורשה ל-Parent בלבד
    // =================================================================

    [HttpPost("submit-health-declaration")]
    [Authorize(Roles = "Parent")]
    public async Task<IActionResult> SubmitHealthDeclaration([FromBody] HealthDeclarationDto declarationDto)
    {
        var authenticatedChildIdNumber = User.Claims.FirstOrDefault(c => c.Type == "ChildIdNumber")?.Value;

        if (authenticatedChildIdNumber != declarationDto.ChildDetails.ChildId.ToString())
        {
            return StatusCode(403, "אינך מורשה לשלוח טופס זה עבור ילד זה.");
        }
        try
        {
            byte[] fileBytes = await _formService.ProcessAndGenerateHealthDeclarationAsync(declarationDto);
            string newFileName = $"Health_Declaration_{declarationDto.ChildDetails.ChildId}.pdf";
            return File(fileBytes, "application/pdf", newFileName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Controller Error: {ex.Message}");
            return StatusCode(500, ex.Message);
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
            if (requestDto == null) return BadRequest("Invalid form data structure.");
        }
        catch (Exception)
        {
            return BadRequest("Failed to parse form data.");
        }

        var authenticatedChildIdNumber = User.Claims.FirstOrDefault(c => c.Type == "ChildIdNumber")?.Value;
        if (authenticatedChildIdNumber != requestDto.StudentDetails.StudentId.ToString())
        {
            return StatusCode(403, "אינך מורשה לשלוח טופס הנחה עבור תלמיד זה.");
        }

        // שמירת קבצים מצורפים
        string allSavedPaths = await CombineAndSaveFiles(formSubmission.GetAttachments());

        try
        {
            byte[] fileBytes = await _formService.ProcessAndGenerateDiscountRequestAsync(requestDto, allSavedPaths);
            string newFileName = $"Discount_Request_{requestDto.StudentDetails.StudentId}_{DateTime.Now:yyyyMMdd}.pdf";
            return File(fileBytes, "application/pdf", newFileName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Controller Error (Discount): {ex.Message}");
            return StatusCode(500, $"שגיאה פנימית: {ex.Message}");
        }
    }

    // פונקציית עזר לשמירת קבצים
    private async Task<string> CombineAndSaveFiles(List<IFormFile> files)
    {
        var savedPaths = new List<string>();
        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                string path = await _fileStorageService.SaveFileAsync(file, AttachmentsFolder);
                savedPaths.Add(path);
            }
        }
        return string.Join(",", savedPaths);
    }

    // =================================================================
    // 🔵 אזור מנהלים (ניהול טפסים) - מורשה ל-Admin בלבד
    // =================================================================

    // 1. קבלת טפסים ממתינים
    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPendingForms()
    {
        var formsEntities = await _formService.GetPendingFormsAsync();
        // המרה ל-DTO שטוח כדי למנוע מעגליות JSON
        var formsDto = formsEntities.Select(MapToDto).ToList();
        return Ok(formsDto);
    }

    // 2. קבלת היסטוריית טפסים שאושרו (הפונקציה שהייתה חסרה לתצוגה התחתונה!)
    [HttpGet("approved")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetApprovedForms()
    {
        var formsEntities = await _formService.GetApprovedFormsAsync();
        var formsDto = formsEntities.Select(MapToDto).ToList();
        return Ok(formsDto);
    }

    // 3. אישור טופס (הפונקציה שהייתה חסרה וגרמה לשגיאה!)
    [HttpPut("approve/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveForm(int id)
    {
        await _formService.ApproveFormAsync(id);
        return Ok(new { message = "הטופס אושר בהצלחה" });
    }

    // פונקציית עזר למיפוי (כדי לא לשכפל קוד)
    private FormManageDto MapToDto(Dal.Models.Form f)
    {
        return new FormManageDto
        {
            FormId = f.FormId,
            FormType = f.FormType,
            Status = f.Status,
            SubmittedDate = f.SubmittedDate,
            FilePath = f.FilePath,

            // שליפת פרטי הילד בצורה בטוחה (Null check)
            ChildFirstName = f.Child?.FirstName ?? "לא ידוע",
            ChildLastName = f.Child?.lastName ?? "",
            ChildIdNumber = f.Child?.IdNumber ?? ""
        };
    }
}