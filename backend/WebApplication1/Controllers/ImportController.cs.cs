using Bo.Interfaces;
using Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
// אין צורך ב-using OfficeOpenXml; שוב אם הוא כבר קיים למעלה.

[ApiController]
[Route("api/[controller]")]
[Authorize] 
[EnableCors("AllowSpecificOrigin")] // ⬅️ הוסף את האטריביוט הזה ל-Controller

public class ImportController : ControllerBase
{
    private readonly IImportService _importService;

    public ImportController(IImportService importService)
    {
        _importService = importService;

        // ******************************************************
        // ** חשוב: השארת הקונסטרקטור נקי **
        // מכיוון שהגדרת הרישיון של EPPlus עברה ל-Program.cs, 
        // אנו מוודאים שאין כאן שורות נוספות שגורמות לשגיאות.
        // ******************************************************
    }

    // פונקציה קיימת לייבוא נתוני ילדים מקובץ אקסל
    [HttpPost("children")]
    public async Task<IActionResult> UploadChildrenExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("יש לבחור קובץ להעלאה.");

        if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest("יש לבחור קובץ בפורמט Excel (.xlsx).");

        List<ParentChildImportDto> mappedData;

        try
        {
            mappedData = await MapExcelToDtoList(file);

            if (!mappedData.Any())
                return BadRequest("הקובץ ריק או שלא נמצאו נתונים למעט כותרות.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"שגיאה בקריאת הקובץ או המרת הנתונים: {ex.Message}");
        }

        try
        {
            await _importService.ImportChildDataAsync(mappedData);
            return Ok($"ייבוא {mappedData.Count} שורות הסתיים בהצלחה. נתונים קיימים עודכנו וחדשים נוספו.");
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest($"שגיאה בייבוא: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"שגיאה פנימית בייבוא הנתונים: {ex.Message}");
        }
    }

    // 🏆 פונקציה חדשה לעדכון/הוספת גנים באמצעות קובץ אקסל
    [HttpPost("kindergarten/excel")]
    public async Task<IActionResult> UploadKindergartensExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("יש לבחור קובץ להעלאה.");

        if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest("יש לבחור קובץ בפורמט Excel (.xlsx).");

        List<KindergartenDto> mappedData;

        try
        {
            mappedData = await MapExcelToKindergartenDtoList(file);

            if (!mappedData.Any())
                return BadRequest("הקובץ ריק או שלא נמצאו נתונים למעט כותרות.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"שגיאה בקריאת הקובץ או המרת הנתונים: {ex.Message}");
        }

        try
        {
            // שימוש בפונקציה הקיימת בסרביס, המטפלת ב-Upsert לפי קוד
            await _importService.ImportKindergartenDataAsync(mappedData);
            return Ok($"ייבוא/עדכון {mappedData.Count} גנים הסתיים בהצלחה.");
        }
        catch (Exception ex)

        {
            return StatusCode(500, $"שגיאה פנימית בייבוא הנתונים: {ex.Message}");
        }
    }

    // 🗺️ מתודת מיפוי עבור גנים
    private async Task<List<KindergartenDto>> MapExcelToKindergartenDtoList(IFormFile file)
    {
        var list = new List<KindergartenDto>();

        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension?.Rows ?? 0;

                // קורא מעמודה 1 (קוד) ומעמודה 2 (שם)
                for (int row = 2; row <= rowCount; row++) // מדלג על כותרת (שורה 1)
                {
                    string code = worksheet.Cells[row, 1].Text?.Trim();
                    string name = worksheet.Cells[row, 2].Text?.Trim();
                    string address = worksheet.Cells[row, 3].Text?.Trim();  

                    if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
                        continue;

                    list.Add(new KindergartenDto
                    {
                        Code = code,
                        Name = name,
                        Address = address
                    });
                }
            }
        }

        return list;
    }

    // מתודת המיפוי הקיימת עבור ילדים (לא שונתה)
    private async Task<List<ParentChildImportDto>> MapExcelToDtoList(IFormFile file)
    {
        var list = new List<ParentChildImportDto>();

        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension?.Rows ?? 0;

                for (int row = 2; row <= rowCount; row++) // מדלג על כותרת
                {

                    string childIdNumber = worksheet.Cells[row, 4].Text?.Trim();
                    string childBirthDateText = worksheet.Cells[row, 6].Text?.Trim();

                    // אם PaymentId הוא שדה חובה, נדלג על השורה אם ההמרה נכשלה
                    // או אם לא היה ערך כלל
                  
                    if (string.IsNullOrWhiteSpace(childIdNumber) || string.IsNullOrWhiteSpace(childBirthDateText))
                        continue;

                    DateTime childBirthDate;
                    if (!DateTime.TryParse(childBirthDateText, out childBirthDate))
                        continue;

                    list.Add(new ParentChildImportDto
                    {
                        KindergartenId = worksheet.Cells[row, 1].Text?.Trim(),
                        Phone = worksheet.Cells[row, 2].Text?.Trim(),
                        Email = worksheet.Cells[row, 3].Text?.Trim(),
                        IdNumber = worksheet.Cells[row, 4].Text?.Trim(),
                        FirstName = worksheet.Cells[row, 5].Text?.Trim(),
                        BirthDate = childBirthDate,
                        SchoolYear = worksheet.Cells[row, 7].Text?.Trim(),
                        FormLink = worksheet.Cells[row, 8].Text?.Trim(),
                        LastName = worksheet.Cells[row, 9].Text?.Trim(),
                    });
                }
            }
        }

        return list;
    }
}