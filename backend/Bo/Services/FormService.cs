using Bo.Interfaces;
using Dto;
using Dal.Repositories.Interfaces;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using System.Diagnostics;
using Dal.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

public class FormService : IFormService
{
    private readonly IFormRepository _formRepository;
    private readonly IConfiguration _configuration;

    private const string ScriptsFolder = "Scripts";
    private const string PermanentFormsFolder = "PermanentForms"; // תיקייה קבועה לשמירת PDF
    private const string PythonScriptName = "generate_pdf_from_docx.py";
    private const string TemplateFileName = "health_declaration_template.docx";


    public FormService(IFormRepository formRepository, IConfiguration configuration)
    {
        _formRepository = formRepository;
        _configuration = configuration;
    }

    public async Task<byte[]> ProcessAndGenerateHealthDeclarationAsync(HealthDeclarationDto declarationDto)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        // הגדרת נתיבים
        string pythonScriptPath = Path.Combine(baseDirectory, ScriptsFolder, PythonScriptName);
        string templatePath = Path.Combine(baseDirectory, "Templates", TemplateFileName);

        string pythonExecutable = _configuration["AppSettings:PythonExecutablePath"] ?? "python";
        string libreOfficeExecutable = _configuration["AppSettings:LibreOfficeExecutablePath"] ?? "soffice";

        // יצירת נתיב קבוע לשמירת הקובץ
        var permanentDirectory = Path.Combine(baseDirectory, PermanentFormsFolder);
        Directory.CreateDirectory(permanentDirectory);

        // שם הקובץ הסופי
        var pdfFileName = $"declaration_{declarationDto.ChildDetails.ChildId}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.pdf";
        var permanentPdfPath = Path.Combine(permanentDirectory, pdfFileName);

        // ** שינוי קריטי: יצירת URL מלא לגישה ציבורית **
        // נניח שיש לך קונטרולר בשם 'Files' ופעולה בשם 'DownloadForm'.
        // ה-Controller הזה צריך לדעת לקבל את ה-pdfFileName ולשלוף את הקובץ מהדיסק.
        string baseUrl = _configuration["AppSettings:BaseUrl"] ?? "http://localhost:5000/"; // לדוגמה
        string formLink = $"{baseUrl}api/Files/DownloadForm/{pdfFileName}";


        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template file not found: {templatePath}. Make sure it is copied to the output folder.");
        }

        // 2. הכנת הנתונים ל-JSON (אין שינוי)
        var dataForPython = new
        {
            form_data = declarationDto,
            output_pdf_path = permanentPdfPath, // שומרים ישר לנתיב הקבוע
            template_path = templatePath,
            libre_office_path = libreOfficeExecutable
        };
        var jsonInput = JsonSerializer.Serialize(dataForPython, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });


        // 3. הפעלת סקריפט הפייתון (אין שינוי)
        await RunPythonScript(pythonExecutable, pythonScriptPath, jsonInput);

        // 4. קריאת קובץ ה-PDF שנוצר (אין שינוי)
        if (!File.Exists(permanentPdfPath))
        {
            throw new FileNotFoundException("PDF file was not created by the Python script. Check Python/soffice logs in the console.");
        }

        byte[] pdfBytes = await File.ReadAllBytesAsync(permanentPdfPath);

        // 5. שמירת הקישור לטופס בטבלת Child
        Console.WriteLine($"Saving link for child ID: {declarationDto.ChildDetails.ChildId} → {formLink}");
        await _formRepository.UpdateChildFormLinkAsync(declarationDto.ChildDetails.ChildId, formLink);


        // 6. מחזירים את הבתים להורדה ללקוח
        return pdfBytes;
    }

    // פונקציה מבודדת להרצת הפייתון (ללא שינוי)
    private async Task RunPythonScript(string pythonExecutable, string pythonScriptPath, string jsonInput)
    {
        ProcessStartInfo start = new ProcessStartInfo
        {
            FileName = pythonExecutable,
            Arguments = pythonScriptPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            CreateNoWindow = true,
        };

        using (Process process = Process.Start(start))
        {
            await process.StandardInput.WriteAsync(jsonInput);
            process.StandardInput.Close();

            string result = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();

            process.WaitForExit(30000);

            if (process.ExitCode != 0)
            {
                string fullError = string.IsNullOrEmpty(error) ? result : error;
                throw new Exception($"Python script failed (Exit Code {process.ExitCode}). Details: {fullError}");
            }
        }
    }
}