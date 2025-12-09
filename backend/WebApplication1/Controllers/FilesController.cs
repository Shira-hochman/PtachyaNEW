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
    public FilesController() { }

    // ⭐️ פונקציה גנרית להורדת קובץ מכל תיקייה
    // Route: api/Files/Download/{container}/{fileName}
    [HttpGet("Download/{container}/{fileName}")]
    public async Task<IActionResult> DownloadFile(string container, string fileName)
    {
        try
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // בניית הנתיב: תיקיית בסיס -> שם התיקייה (Container) -> שם הקובץ
            var fullPath = Path.Combine(baseDirectory, container, fileName);

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound($"File not found: {fileName} in {container}");
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);

            // החזרת הקובץ (PDF כברירת מחדל, אפשר לשכלל לפי סיומת)
            string contentType = "application/pdf";
            if (fileName.EndsWith(".jpg") || fileName.EndsWith(".jpeg")) contentType = "image/jpeg";
            if (fileName.EndsWith(".png")) contentType = "image/png";

            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading file: {ex.Message}");
            return StatusCode(500, "Internal error while fetching file.");
        }
    }
}