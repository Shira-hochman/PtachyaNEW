// FormService.cs - קוד סופי ומוכן לענן ולבדיקת PDF

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

    // ⭐️ התיקון הקריטי: תיקון השם ל-"generate" ⭐️
    private const string ScriptsFolder = "Scripts";
    private const string PythonScriptName = "generate_pdf_from_docx.py";
    private const string TemplateFileName = "health_declaration_template.docx";


    public FormService(IFormRepository formRepository, IConfiguration configuration)
    {
        _formRepository = formRepository;
        _configuration = configuration;
    }

    public async Task<byte[]> ProcessAndGenerateHealthDeclarationAsync(HealthDeclarationDto declarationDto)
    {
        // 1. קביעת נתיבים גמישים וחוצי-פלטפורמות
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        string pythonScriptPath = Path.Combine(baseDirectory, ScriptsFolder, PythonScriptName);
        string templatePath = Path.Combine(baseDirectory, "Templates", TemplateFileName);

        // קריאת הנתיבים הגמישים מתוך appsettings.json
        string pythonExecutable = _configuration["AppSettings:PythonExecutablePath"] ?? "python";
        string libreOfficeExecutable = _configuration["AppSettings:LibreOfficeExecutablePath"] ?? "soffice";

        // יצירת נתיב זמני ל-PDF
        var tempDirectory = Path.Combine(Path.GetTempPath(), "PtachyaForms");
        Directory.CreateDirectory(tempDirectory);
        var pdfFileName = $"declaration_{declarationDto.ChildDetails.ChildId}_{DateTime.Now.Ticks}.pdf";
        var tempPdfPath = Path.Combine(tempDirectory, pdfFileName);

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template file not found: {templatePath}. Make sure it is copied to the output folder.");
        }

        // 2. הכנת הנתונים ל-JSON
        var dataForPython = new
        {
            form_data = declarationDto,
            output_pdf_path = tempPdfPath,
            template_path = templatePath,
            libre_office_path = libreOfficeExecutable // נתיב SOFFICE.EXE עובר לפייתון
        };
        var jsonInput = JsonSerializer.Serialize(dataForPython, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });


        // 3. הפעלת סקריפט הפייתון
        await RunPythonScript(pythonExecutable, pythonScriptPath, jsonInput);

        // 4. קריאת קובץ ה-PDF שנוצר
        if (!File.Exists(tempPdfPath))
        {
            throw new FileNotFoundException("PDF file was not created by the Python script. Check Python/soffice logs in the console.");
        }

        byte[] pdfBytes = await File.ReadAllBytesAsync(tempPdfPath);

        // ⭐️⭐️⭐️ ביטול זמני של שמירת ה-SQL ⭐️⭐️⭐️
        /* // יש להסיר את הבלוק הזה כאשר תרצי לחזור לשמירה ב-SQL 
        if (int.TryParse(declarationDto.ChildDetails.ChildId, out int childIdInt))
        {
             var formEntity = new Form
             {
                 ChildId = childIdInt, 
                 FileContent = pdfBytes,
                 ContentType = "application/pdf",
                 SubmittedDate = DateTime.Now
             };
             await _formRepository.AddAsync(formEntity);
        }
        */

        // 5. ניקוי והחזרת הבתים
        File.Delete(tempPdfPath);
        return pdfBytes;
    }

    // פונקציה מבודדת להרצת הפייתון
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