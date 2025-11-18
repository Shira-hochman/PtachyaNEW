using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using System;

[ApiController]
[Route("api/[controller]")]
[EnableCors("AllowSpecificOrigin")]
public class FilesController : ControllerBase
{
    private const string PermanentFormsFolder = "PermanentForms";

    public FilesController() { }

    [HttpGet("DownloadForm/{fileName}")]
    public async Task<IActionResult> DownloadForm(string fileName)
    {
        try
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            // בונים את הנתיב המוחלט לקובץ על הדיסק, באמצעות שם הקובץ שנשלח ב-URL
            var permanentDirectory = Path.Combine(baseDirectory, PermanentFormsFolder);
            var fullPath = Path.Combine(permanentDirectory, fileName);

            if (!System.IO.File.Exists(fullPath))
            {
                // אם הקובץ לא נמצא על הדיסק, נחזיר 404
                return NotFound($"File not found at path: {fullPath}");
            }

            // קוראים את תוכן הקובץ לתוך מערך בתים
            var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);

            // מחזירים את הקובץ ללקוח עם Content-Type מתאים
            return File(fileBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error serving file {fileName}: {ex.Message}");
            return StatusCode(500, "Internal error while fetching file.");
        }
    }
    // ⭐️⭐️⭐️ הוספת המתודה החדשה לטיפול בטופס הנחה ⭐️⭐️⭐️
    // Route: api/Files/DownloadDiscountForm/{fileName}
    [HttpGet("DownloadDiscountForm/{fileName}")]
    public async Task<IActionResult> DownloadDiscountForm(string fileName)
    {
        // הלוגיקה זהה לחלוטין למתודת ה-DownloadForm הקיימת, כיוון ששני הקבצים 
        // נשמרים באותה תיקייה (PermanentFormsFolder).
        try
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var permanentDirectory = Path.Combine(baseDirectory, PermanentFormsFolder);
            var fullPath = Path.Combine(permanentDirectory, fileName);

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound($"Discount form file not found at path: {fullPath}");
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            return File(fileBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error serving discount file {fileName}: {ex.Message}");
            return StatusCode(500, "Internal error while fetching discount file.");
        }
    }
}