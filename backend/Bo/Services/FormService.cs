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

    // ⭐️⭐️⭐️ הגדרת שני סקריפטים נפרדים ⭐️⭐️⭐️
    private const string HealthPythonScriptName = "generate_pdf_from_docx.py"; // סקריפט מורכב ישן
    private const string DiscountPythonScriptName = "process_discount_request.py"; // סקריפט חדש נקי

    // ⭐️ שמות תבניות
    private const string HealthTemplateFileName = "health_declaration_template.docx";
    private const string DiscountTemplateFileName = "רקע.docx";


    public FormService(IFormRepository formRepository, IConfiguration configuration)
    {
        _formRepository = formRepository;
        _configuration = configuration;
    }

    // ⭐️ שיטה קיימת: הצהרת בריאות (משתמש ב-generate_pdf_from_docx.py)
    public async Task<byte[]> ProcessAndGenerateHealthDeclarationAsync(HealthDeclarationDto declarationDto)
    {
        return await ProcessAndGenerateFormAsync(
            declarationDto,
            HealthTemplateFileName,
            HealthPythonScriptName, // 🛑 שימוש בסקריפט הבריאות
            $"declaration_{declarationDto.ChildDetails.ChildId}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.pdf",
            // פונקציה לשמירת לינק
            async (pdfFileName, childId) =>
            {
                string baseUrl = _configuration["AppSettings:BaseUrl"] ?? "http://localhost:5000/";
                string formLink = $"{baseUrl}api/Files/DownloadForm/{pdfFileName}";
                Console.WriteLine($"Saving link for child ID: {childId} → {formLink}");
                await _formRepository.UpdateChildFormLinkAsync(childId, formLink);
            }
        );
    }

    // ⭐️⭐️⭐️ שיטה חדשה: בקשת הנחה (משתמש ב-process_discount_request.py) ⭐️⭐️⭐️
    // ⭐️⭐️⭐️ שיטה חדשה: בקשת הנחה (כעת מקבלת את נתיבי הקבצים) ⭐️⭐️⭐️
    public async Task<byte[]> ProcessAndGenerateDiscountRequestAsync(DiscountRequestDto requestDto, string uploadedPaths)
    {
        // הפונקציה ProcessAndGenerateFormAsync מקבלת:
        // 1. formData
        // 2. templateFileName
        // 3. pythonScriptName
        // 4. pdfFileName
        // 5. postProcessAction (ה-Lambda Function ששומרת לינקים)

        return await ProcessAndGenerateFormAsync(
            requestDto,
            DiscountTemplateFileName,
            DiscountPythonScriptName,
            $"discount_request_{requestDto.StudentDetails.StudentId}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.pdf",

            // ⭐️⭐️⭐️ פונקציית שמירת לינק ונתיבים (הגרסה הנכונה) ⭐️⭐️⭐️
            async (pdfFileName, childId) =>
            {
                string baseUrl = _configuration["AppSettings:BaseUrl"] ?? "http://localhost:5000/";
                string formLink = $"{baseUrl}api/Files/DownloadDiscountForm/{pdfFileName}";

                // 1. שמירת קישור ה-PDF הראשי
                await _formRepository.UpdateChildDiscountLinkAsync(childId, formLink);

                // 2. שמירת נתיבי הקבצים הנוספים (CSV)
                await _formRepository.UpdateChildDocumentPathsAsync(childId, uploadedPaths);
            }
        ); // סגירת הסוגר של return await ProcessAndGenerateFormAsync(...);
    }
    // סגירת הסוגר של המתודה
    // ⭐️⭐️⭐️ פונקציית עזר כללית לכל סוגי הטפסים ⭐️⭐️⭐️
    private async Task<byte[]> ProcessAndGenerateFormAsync<T>(
        T formData,
        string templateFileName,
        string pythonScriptName, // ⭐️ פרמטר חדש
        string pdfFileName,
        Func<string, int, Task>? postProcessAction) where T : class
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        // ⭐️ שימוש בשם הסקריפט שהתקבל כפרמטר
        string pythonScriptPath = Path.Combine(baseDirectory, ScriptsFolder, pythonScriptName);
        string templatePath = Path.Combine(baseDirectory, "Templates", templateFileName);

        string pythonExecutable = _configuration["AppSettings:PythonExecutablePath"] ?? "python";
        string libreOfficeExecutable = _configuration["AppSettings:LibreOfficeExecutablePath"] ?? "soffice";

        var permanentDirectory = Path.Combine(baseDirectory, PermanentFormsFolder);
        Directory.CreateDirectory(permanentDirectory);

        var permanentPdfPath = Path.Combine(permanentDirectory, pdfFileName);

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template file not found: {templatePath}. Make sure it is copied to the output folder.");
        }

        // הכנת הנתונים ל-JSON
        var dataForPython = new
        {
            form_data = formData,
            output_pdf_path = permanentPdfPath,
            template_path = templatePath,
            libre_office_path = libreOfficeExecutable
        };
        var jsonInput = JsonSerializer.Serialize(dataForPython, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        // הפעלת סקריפט הפייתון
        await RunPythonScript(pythonExecutable, pythonScriptPath, jsonInput);

        if (!File.Exists(permanentPdfPath))
        {
            throw new FileNotFoundException("PDF file was not created by the Python script. Check Python/soffice logs in the console.");
        }

        byte[] pdfBytes = await File.ReadAllBytesAsync(permanentPdfPath);

       
        // 5. שמירת הקישור לטופס אם נדרש
        if (postProcessAction != null)
        {
            // הנחה: לטופס HealthDeclarationDto קיים ChildDetailsDto עם ChildId (int)
            if (formData is HealthDeclarationDto healthDto)
            {
                await postProcessAction(pdfFileName, healthDto.ChildDetails.ChildId);
            }
            // ⭐️⭐️⭐️ הוספת טיפול בטופס הנחה ⭐️⭐️⭐️
            else if (formData is DiscountRequestDto discountDto)
            {
                // ⭐️⭐️⭐️ תיקון: המרה מ-string ל-int ⭐️⭐️⭐️
                if (int.TryParse(discountDto.StudentDetails.StudentId, out int childIdInt))
                {
                    // מעביר את תעודת הזהות כ-int
                    await postProcessAction(pdfFileName, childIdInt);
                }
                else
                {
                    // במקרה שתעודת הזהות אינה מספר (למרות שהאנגולר אמור למנוע זאת)
                    throw new ArgumentException($"StudentId '{discountDto.StudentDetails.StudentId}' is not a valid integer for child ID conversion.");
                }
            }
        }

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