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
}