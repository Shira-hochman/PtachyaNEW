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
using NPOI.POIFS.Properties;

public class FormService : IFormService
{
    private readonly IFormRepository _formRepository;
    private readonly IConfiguration _configuration;
    private readonly IFileStorageService _fileStorageService;

    private const string ScriptsFolder = "Scripts";
    private const string PermanentFormsFolder = "PermanentForms"; // תיקייה קבועה לשמירת PDF

    // ⭐️⭐️⭐️ הגדרת שני סקריפטים נפרדים ⭐️⭐️⭐️
    private const string HealthPythonScriptName = "generate_pdf_from_docx.py"; // סקריפט מורכב ישן
    private const string DiscountPythonScriptName = "process_discount_request.py"; // סקריפט חדש נקי

    // ⭐️ שמות תבניות
    private const string HealthTemplateFileName = "health_declaration_template.docx";
    private const string DiscountTemplateFileName = "רקע.docx";


    public FormService(IFormRepository formRepository, IConfiguration configuration, IFileStorageService fileStorageService) // ⭐️ הוספה לקונסטרוקטור
    {
        _formRepository = formRepository;
        _configuration = configuration;
        _fileStorageService = fileStorageService; // ⭐️ שמירה בשדה פרטי
    }

    public async Task ApproveFormAsync(int formId)
    {
        await _formRepository.ApproveFormAsync(formId);
    }

    public async Task<List<Form>> GetPendingFormsAsync()
    {
        return await _formRepository.GetPendingFormsAsync();
    }

    public async Task<List<Form>> GetApprovedFormsAsync()
    {
        return await _formRepository.GetApprovedFormsAsync();
    }
    // ⭐️ שיטה קיימת: הצהרת בריאות
    // FormService.cs

    public async Task<byte[]> ProcessAndGenerateHealthDeclarationAsync(HealthDeclarationDto declarationDto)
    {
        // 1. ⭐️ שלב קריטי: המרת ChildId (ת"ז) ל-string
        string idNumber = declarationDto.ChildDetails.ChildId.ToString();

        // 2. ⭐️ איתור המפתח הראשי ChildId ב-DB לפי תעודת הזהות
        int? actualChildPK = await _formRepository.GetChildPkByIdNumberAsync(idNumber);

        if (!actualChildPK.HasValue)
        {
            // 🛑 שגיאה: הילד לא קיים במסד הנתונים
            throw new ArgumentException($"Child with ID number {idNumber} not found in database.");
        }

        int childPK = actualChildPK.Value;

        // 3. שימוש ב-PK הנכון (childPK) בכל מקום
        return await ProcessAndGenerateFormAsync(
            declarationDto,
            HealthTemplateFileName,
            HealthPythonScriptName,
            $"declaration_{childPK}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.pdf", // ⬅️ שימוש ב-PK

            async (finalPath, childId) =>
            {
                var newFormEntry = new Form
                {
                    ChildId = childPK, // ⬅️ ⭐️ שימוש ב-PK הנכון לשמירה!
                    FormType = "HEALTH_DECLARATION",
                    FilePath = finalPath,
                    SubmittedDate = DateTime.Now,
                    ContentType = "application/pdf"
                };
                await _formRepository.AddAsync(newFormEntry);
                Console.WriteLine($"Form record created for Health Declaration. Child ID: {childPK}");
            }
        );
    }

    // ⭐️⭐️⭐️ שיטה חדשה: בקשת הנחה ⭐️⭐️⭐️
    // FormService.cs - מתודת ProcessAndGenerateDiscountRequestAsync

    public async Task<byte[]> ProcessAndGenerateDiscountRequestAsync(DiscountRequestDto requestDto, string uploadedPaths)
    {
        // 1. ⭐️ שלב קריטי: איתור המפתח הראשי (PK) לפי תעודת הזהות ⭐️
        string idNumber = requestDto.StudentDetails.StudentId;

        // 2. איתור המפתח הראשי ChildId ב-DB
        // 💡 הערה: חובה לוודא שפונקציה זו קיימת ב-IFormRepository
        int? actualChildPK = await _formRepository.GetChildPkByIdNumberAsync(idNumber);

        if (!actualChildPK.HasValue)
        {
            // 🛑 אם הילד לא קיים, נזרק שגיאה
            throw new ArgumentException($"Child (Student) with ID number {idNumber} not found in database.");
        }

        int childPK = actualChildPK.Value; // ⬅️ זהו המפתח הראשי (PK) הנכון!

        // 3. שימוש ב-PK הנכון (childPK) בהמשך הקוד
        return await ProcessAndGenerateFormAsync(
            requestDto,
            DiscountTemplateFileName,
            DiscountPythonScriptName,
            // ⬅️ שימוש ב-PK בשם הקובץ
            $"discount_request_{childPK}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.pdf",

            // ⭐️⭐️⭐️ ה-Lambda Function לשמירת ה-DB ⭐️⭐️⭐️
            async (finalPath, _) => // ⬅️ הפרמטר childId לא נחוץ כאן, משתמשים ב-childPK
            {
                var newFormEntry = new Form
                {
                    ChildId = childPK, // ⬅️ ⭐️ שימוש ב-PK הנכון לשמירה!
                    FormType = "DISCOUNT_REQUEST",
                    FilePath = finalPath,
                    AttachmentPaths = uploadedPaths,
                    SubmittedDate = DateTime.Now,
                    ContentType = "application/pdf"
                };
                await _formRepository.AddAsync(newFormEntry);
                Console.WriteLine($"Form record created for Discount Request. Child ID: {childPK}");
            }
        );
    }


    // ⭐️⭐️⭐️ פונקציית עזר כללית לכל סוגי הטפסים - גרסה סופית ⭐️⭐️⭐️
    // ... (שאר הקוד בקובץ נשאר זהה)

    private async Task<byte[]> ProcessAndGenerateFormAsync<T>(
        T formData,
        string templateFileName,
        string pythonScriptName,
        string pdfFileName,
        Func<string, int, Task>? postProcessAction) where T : class
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        string pythonScriptPath = Path.Combine(baseDirectory, ScriptsFolder, pythonScriptName);
        string templatePath = Path.Combine(baseDirectory, "Templates", templateFileName);
        string pythonExecutable = _configuration["AppSettings:PythonExecutablePath"] ?? "python";
        string libreOfficeExecutable = _configuration["AppSettings:LibreOfficeExecutablePath"] ?? "soffice";

        // ⭐️ תיקון: שימוש בתיקייה זמנית נפרדת ליצירה הראשונית
        var tempDirectory = Path.Combine(baseDirectory, "TempGeneration");
        Directory.CreateDirectory(tempDirectory); // מוודא שהיא קיימת
        var tempPdfPath = Path.Combine(tempDirectory, pdfFileName); // הקובץ הזמני

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template file not found: {templatePath}.");
        }

        var dataForPython = new
        {
            form_data = formData,
            output_pdf_path = tempPdfPath, // ⬅️ שולחים לפייתון את הנתיב הזמני
            template_path = templatePath,
            libre_office_path = libreOfficeExecutable
        };
        var jsonInput = JsonSerializer.Serialize(dataForPython, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        await RunPythonScript(pythonExecutable, pythonScriptPath, jsonInput);
        Console.WriteLine("DEBUG: Python script finished.");

        // בדיקת קיום והמתנה (על הנתיב הזמני)
        int maxAttempts = 5;
        int delayMs = 200;
        bool fileFound = false;

        for (int i = 0; i < maxAttempts; i++)
        {
            if (File.Exists(tempPdfPath)) // ⬅️ בודקים בתיקייה הזמנית
            {
                fileFound = true;
                break;
            }
            await Task.Delay(delayMs);
        }

        if (!fileFound)
        {
            throw new FileNotFoundException("PDF file was not created by the Python script.");
        }

        // 1. קריאת הקובץ מהתיקייה הזמנית
        byte[] pdfBytes = await File.ReadAllBytesAsync(tempPdfPath);

        // 2. שמירה בתיקייה הקבועה (PermanentForms) דרך הסרוויס
        // (זה ייצור את הקובץ במיקום שראית בצילום המסך)
        string finalPath = await _fileStorageService.SaveBytesAsync(
            pdfBytes,
            pdfFileName,
            PermanentFormsFolder
        );

        // 3. מחיקת הקובץ מהתיקייה הזמנית בלבד
        try
        {
            File.Delete(tempPdfPath); // ⬅️ מוחק מ-TempGeneration, לא מ-PermanentForms
            Console.WriteLine("DEBUG: Deleted temporary PDF file.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WARNING: Could not delete temp file. Error: {ex.Message}");
        }

        // 4. שמירת רשומה ב-DB
        if (postProcessAction != null)
        {
            if (formData is HealthDeclarationDto healthDto)
            {
                await postProcessAction(finalPath, healthDto.ChildDetails.ChildId);
            }
            else if (formData is DiscountRequestDto discountDto)
            {
                if (int.TryParse(discountDto.StudentDetails.StudentId, out int childIdInt))
                {
                    await postProcessAction(finalPath, childIdInt);
                }
                else
                {
                    throw new ArgumentException($"Invalid StudentId: {discountDto.StudentDetails.StudentId}");
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