using Bo.Interfaces;
using Dal.Models;
using Dal.Repositories.Interfaces;
using Dto;
using Microsoft.Extensions.Configuration;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public class FormService : IFormService
{
    private readonly IFormRepository _formRepository;
    private readonly IConfiguration _configuration;
    private readonly IFileStorageService _fileStorageService;

    private const string PermanentFormsFolder = "PermanentForms";
    private const string HealthTemplateHtml = "Templates/HealthDeclaration.html";
    private const string DiscountTemplateHtml = "Templates/DiscountRequest.html";

    public FormService(IFormRepository formRepository, IConfiguration configuration, IFileStorageService fileStorageService)
    {
        _formRepository = formRepository;
        _configuration = configuration;
        _fileStorageService = fileStorageService;
    }

    public async Task ApproveFormAsync(int formId) => await _formRepository.ApproveFormAsync(formId);
    public async Task<List<Form>> GetPendingFormsAsync() => await _formRepository.GetPendingFormsAsync();
    public async Task<List<Form>> GetApprovedFormsAsync() => await _formRepository.GetApprovedFormsAsync();

    // ---------------------------------------------------------
    // 🟢 טופס 1: הצהרת בריאות
    // ---------------------------------------------------------
    public async Task<byte[]> ProcessAndGenerateHealthDeclarationAsync(HealthDeclarationDto declarationDto)
    {
        string idNumber = declarationDto.ChildDetails.ChildId.ToString();
        int? actualChildPK = await _formRepository.GetChildPkByIdNumberAsync(idNumber);

        if (!actualChildPK.HasValue)
            throw new ArgumentException($"Child with ID number {idNumber} not found in database.");

        int childPK = actualChildPK.Value;

        // ⭐️ שליפת המייל של הילד מה-DB (יש לוודא שהמתודה קיימת ב-Repository)
        string parentEmail = await _formRepository.GetChildEmailByIdAsync(childPK);

        // טעינת תבנית
        string htmlContent = await LoadTemplateAsync(HealthTemplateHtml);

        // הכנת חתימה
        // 1. הכנת החתימות (המרה ל-HTML)
        string signature1Html = ConvertSignatureToHtml(declarationDto.Parent1.Signature);
        string signature2Html = !string.IsNullOrEmpty(declarationDto.Parent2.Signature)
                                ? ConvertSignatureToHtml(declarationDto.Parent2.Signature)
                                : "";

        // 2. החלפת הנתונים ב-htmlContent
        htmlContent = htmlContent
            .Replace("{{FormDate}}", declarationDto.FormDate.ToString("dd/MM/yyyy"))
            .Replace("{{StudentName}}", $"{declarationDto.ChildDetails.ChildFirstName} {declarationDto.ChildDetails.ChildLastName}")
            .Replace("{{StudentId}}", declarationDto.ChildDetails.ChildId.ToString())
            .Replace("{{StudentDob}}", declarationDto.ChildDetails.ChildDob.ToString("dd/MM/yyyy"))
            .Replace("{{StudentAddress}}", declarationDto.ChildDetails.ChildAddress)
            .Replace("{{ProgramProvider}}", declarationDto.ProgramProvider ?? "___________")
            .Replace("{{ProgramFramework}}", declarationDto.ProgramFramework ?? "___________")
            .Replace("{{FacilityName}}", declarationDto.FacilityDetails.FacilityName)
            .Replace("{{FacilityOwnership}}", declarationDto.FacilityDetails.FacilityOwnership)
            .Replace("{{FacilityManagerName}}", declarationDto.FacilityDetails.FacilityManagerName)
            .Replace("{{FacilityPhone}}", declarationDto.FacilityDetails.FacilityPhone)
            .Replace("{{FacilityAddress}}", declarationDto.FacilityDetails.FacilityAddress)
            .Replace("{{ParticipationAmount}}", declarationDto.MonthlySelfParticipation.ToString())

            // נתוני הורה 1
            .Replace("{{ParentName}}", declarationDto.Parent1.Name)
            .Replace("{{SignatureImageTag}}", signature1Html)

            // נתוני הורה 2 (החדשים)
            .Replace("{{Parent2Name}}", declarationDto.Parent2.Name ?? "___________")
            .Replace("{{Signature2ImageTag}}", signature2Html);
        // יצירה ושמירה
        byte[] pdfBytes = await GeneratePdfFromHtmlAsync(htmlContent);
        await SaveFormRecordAsync(childPK, "HEALTH_DECLARATION", pdfBytes);

        // ⭐️ שליחת המייל באופן אוטומטי להורה ⭐️
        if (!string.IsNullOrEmpty(parentEmail))
        {
            string childFullName = $"{declarationDto.ChildDetails.ChildFirstName} {declarationDto.ChildDetails.ChildLastName}";
            await SendPdfByEmailAsync(parentEmail, childFullName, pdfBytes);
        }

        return pdfBytes;
    }

    // ---------------------------------------------------------
    // 🔵 טופס 2: בקשת הנחה
    // ---------------------------------------------------------
    // ---------------------------------------------------------
    // 🔵 טופס 2: בקשת הנחה - הפונקציה המלאה עם שליחת מייל
    // ---------------------------------------------------------
    public async Task<byte[]> ProcessAndGenerateDiscountRequestAsync(DiscountRequestDto requestDto, string uploadedPaths)
    {
        // 1. זיהוי הילד במערכת ושליפת המייל של ההורה
        string idNumber = requestDto.StudentDetails.StudentId;
        int? actualChildPK = await _formRepository.GetChildPkByIdNumberAsync(idNumber);

        if (!actualChildPK.HasValue)
            throw new ArgumentException($"Child with ID {idNumber} not found.");

        int childPK = actualChildPK.Value;

        // שליפת המייל מה-DB לצורך השליחה בסוף התהליך
        string parentEmail = await _formRepository.GetChildEmailByIdAsync(childPK);

        // 2. טעינת תבנית ה-HTML והכנת הנתונים
        string htmlContent = await LoadTemplateAsync(DiscountTemplateHtml);
        string signatureHtml = ConvertSignatureToHtml(requestDto.ParentSignature);
        string childrenRows = GenerateChildrenTableRows(requestDto.ChildrenInCustody);

        string xMark = "X";
        string emptyMark = "";

        var reasons = requestDto.DiscountReasons;
        var income = requestDto.LowIncomeDetails;
        var student = requestDto.StudentDetails;

        // 3. החלפת התגיות בתוכן האמיתי
        htmlContent = htmlContent
            .Replace("{{StudentName}}", student.StudentName)
            .Replace("{{StudentId}}", student.StudentId)
            .Replace("{{Kindergarten}}", student.Kindergarten)
            .Replace("{{City}}", student.City)
            .Replace("{{DeclarantName}}", requestDto.DeclarantName)
            .Replace("{{DeclarantId}}", requestDto.DeclarantId)
            .Replace("{{MaritalStatus}}", requestDto.MaritalStatus)
            .Replace("{{ChildrenCount}}", requestDto.ChildrenCount.ToString())
            .Replace("{{ChildrenTableRows}}", childrenRows)
            .Replace("{{CheckLowIncome}}", reasons.LowIncome ? xMark : emptyMark)
            .Replace("{{CheckSiblingPtachya}}", emptyMark)
            .Replace("{{SiblingPtachyaName}}", "")
            .Replace("{{SiblingPtachyaId}}", "")
            .Replace("{{CheckSiblingOther}}", reasons.OtherChildSpecialEd ? xMark : emptyMark)
            .Replace("{{CheckSocialWorker}}", reasons.SocialWorkerRec ? xMark : emptyMark)
            .Replace("{{Spouse1Name}}", "בן/בת הזוג 1")
            .Replace("{{Spouse2Name}}", "בן/בת הזוג 2")
            .Replace("{{Spouse1Status}}", income?.Spouse1Status ?? "")
            .Replace("{{Spouse2Status}}", income?.Spouse2Status ?? "")
            .Replace("{{Spouse1AvgIncome}}", income?.Spouse1AvgMonthlyIncome?.ToString() ?? "")
            .Replace("{{Spouse2AvgIncome}}", income?.Spouse2AvgMonthlyIncome?.ToString() ?? "")
            .Replace("{{Spouse1Total}}", income?.Spouse1Total3Months?.ToString() ?? "")
            .Replace("{{Spouse2Total}}", income?.Spouse2Total3Months?.ToString() ?? "")
            .Replace("{{ReasoningText}}", requestDto.Reasoning ?? "")
            .Replace("{{FormDate}}", requestDto.FormDate.ToString("dd/MM/yyyy"))
            .Replace("{{SignatureImageTag}}", signatureHtml);

        // 4. יצירת קובץ ה-PDF ושמירתו בשרת/בסיס הנתונים
        byte[] pdfBytes = await GeneratePdfFromHtmlAsync(htmlContent);
        await SaveFormRecordAsync(childPK, "DISCOUNT_REQUEST", pdfBytes, uploadedPaths);

        // 5. ⭐️ שליחת המייל האוטומטי להורה ⭐️
        if (!string.IsNullOrEmpty(parentEmail))
        {
            try
            {
                string childFullName = student.StudentName;
                // שים לב: הוספנו כאן את הפרמטר "בקשת הנחה" כדי שהמייל יהיה ברור
                await SendPdfByEmailAsync(parentEmail, childFullName, pdfBytes, "בקשת הנחה");
            }
            catch (Exception ex)
            {
                // כתיבת שגיאה ללוג בלבד כדי שהתהליך הראשי לא ייעצר אם המייל נכשל
                Console.WriteLine($"שגיאה בשליחת מייל עבור בקשת הנחה: {ex.Message}");
            }
        }

        return pdfBytes;
    }

    // ---------------------------------------------------------
    // 🛠️ פונקציות עזר (Helper Methods)
    // ---------------------------------------------------------

    private async Task<string> LoadTemplateAsync(string relativePath)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string templatePath = Path.Combine(baseDirectory, relativePath);

        if (!File.Exists(templatePath))
        {
            string projectPath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);
            if (File.Exists(projectPath)) templatePath = projectPath;
            else if (!File.Exists(templatePath))
                templatePath = Path.GetFullPath(Path.Combine(baseDirectory, $@"..\..\..\..\Bo\{relativePath}"));
        }

        if (!File.Exists(templatePath))
            throw new FileNotFoundException($"Template not found: {templatePath}");

        return await File.ReadAllTextAsync(templatePath);
    }

    private async Task<byte[]> GeneratePdfFromHtmlAsync(string htmlContent)
    {
        string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "image1.jpeg");
        if (File.Exists(imagePath))
        {
            byte[] imageArray = File.ReadAllBytes(imagePath);
            string base64Image = Convert.ToBase64String(imageArray);
            htmlContent = htmlContent.Replace("url('image1.jpeg')", $"url('data:image/jpeg;base64,{base64Image}')");
        }
        string chromePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
        if (!File.Exists(chromePath)) chromePath = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";

        if (!File.Exists(chromePath))
            throw new FileNotFoundException("Google Chrome not found. Please ensure Chrome is installed.");

        using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            ExecutablePath = chromePath,
            Args = new[] { "--no-sandbox" }
        });

        using var page = await browser.NewPageAsync();
        await page.SetContentAsync(htmlContent);

        return await page.PdfDataAsync(new PdfOptions
        {
            Format = PaperFormat.A4,
            PrintBackground = true,
            MarginOptions = new MarginOptions { Top = "15px", Bottom = "15px", Left = "15px", Right = "15px" }
        });
    }

    private string ConvertSignatureToHtml(string base64Signature)
    {
        if (string.IsNullOrEmpty(base64Signature)) return "";
        string cleanBase64 = base64Signature;
        if (!cleanBase64.StartsWith("data:image"))
            cleanBase64 = $"data:image/png;base64,{cleanBase64}";
        return $"<img src='{cleanBase64}' class='sig-img' />";
    }

    private string GenerateChildrenTableRows(List<ChildInCustodyDto> children)
    {
        string rowsHtml = "";
        for (int i = 0; i < 4; i++)
        {
            var child = (children != null && i < children.Count) ? children[i] : null;
            var name = child?.FirstName ?? "";
            var last = child?.LastName ?? "";
            var id = child?.Id ?? "";
            rowsHtml += $"<tr><td>{i + 1}</td><td>{name}</td><td>{last}</td><td>{id}</td></tr>";
        }
        return rowsHtml;
    }

    private async Task SaveFormRecordAsync(int childId, string formType, byte[] pdfBytes, string attachmentPaths = null)
    {
        string fileName = $"{formType.ToLower()}_{childId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        string finalPath = await _fileStorageService.SaveBytesAsync(pdfBytes, fileName, PermanentFormsFolder);

        var newFormEntry = new Form
        {
            ChildId = childId,
            FormType = formType,
            FilePath = finalPath,
            SubmittedDate = DateTime.Now,
            ContentType = "application/pdf",
            AttachmentPaths = attachmentPaths
        };

        await _formRepository.AddAsync(newFormEntry);
    }

    public async Task<List<ChildFormDto>> GetFormsByIdNumberAsync(string idNumber)
    {
        int? childPk = await _formRepository.GetChildPkByIdNumberAsync(idNumber);
        if (!childPk.HasValue) throw new ArgumentException($"לא נמצא ילד עם תעודת זהות {idNumber}");

        var forms = await _formRepository.GetFormsByChildIdAsync(childPk.Value);
        string baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:7222/";

        return forms.Where(f => !string.IsNullOrEmpty(f.FilePath)).Select(f =>
        {
            string fileName = Path.GetFileName(f.FilePath);

            // --- לוגיקת הפיצול החדשה ---
            var attachments = new List<string>();
            if (!string.IsNullOrEmpty(f.AttachmentPaths))
            {
                attachments = f.AttachmentPaths.Split(',')
                    .Select(path => $"{baseUrl}api/Form/Download?container=DiscountAttachments&fileName={Path.GetFileName(path)}")
                    .ToList();
            }

            return new ChildFormDto
            {
                FormId = f.FormId,
                FormType = f.FormType,
                Status = f.Status,
                FileName = fileName,
                DownloadUrl = $"{baseUrl}api/Form/Download?container={PermanentFormsFolder}&fileName={fileName}",
                UploadDate = f.SubmittedDate ?? DateTime.MinValue,
                AttachmentUrls = attachments // שליחת הרשימה המפורקת
            };
        }).ToList();
    }


    // ---------------------------------------------------------
    // 📧 פונקציית שליחת מייל
    // ---------------------------------------------------------
    // ---------------------------------------------------------
    // 📧 פונקציית שליחת מייל גנרית התומכת בכל סוגי הטפסים
    // ---------------------------------------------------------
    private async Task SendPdfByEmailAsync(string parentEmail, string childName, byte[] pdfBytes, string formTypeName = "הצהרת בריאות")
    {
        try
        {
            // הגדרות חשבון השולח
            string senderEmail = "syrhhwkmn17@gmail.com";
            string senderPassword = "icwxylkjdkstkown"; // קוד האפליקציה (16 תווים) מ-Google

            // הגדרת השרת של Gmail
            using var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true,
            };

            // יצירת הודעת המייל
            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, "מערכת ניהול טפסים"),
                Subject = $"{formTypeName} חתומה - {childName}",
                Body = $"שלום רב,\n\nמצורפת בזאת {formTypeName} עבור {childName}.\n\nבברכה,\nמערכת ניהול טפסים",
                IsBodyHtml = false,
            };

            // הוספת הנמען
            mailMessage.To.Add(parentEmail);

            // טיפול בקובץ המצורף (PDF) מתוך הזיכרון
            using (var ms = new MemoryStream(pdfBytes))
            {
                // יצירת שם קובץ תקין (החלפת רווחים בקו תחתון)
                string safeFileName = formTypeName.Replace(" ", "_");
                var attachment = new Attachment(ms, $"{safeFileName}_{childName}.pdf", "application/pdf");

                mailMessage.Attachments.Add(attachment);

                // שליחת המייל בפועל
                await smtpClient.SendMailAsync(mailMessage);
            }

            Console.WriteLine($"המייל נשלח בהצלחה לכתובת: {parentEmail}");
        }
        catch (Exception ex)
        {
            // רישום השגיאה ללוג - לא נרצה להפיל את כל השרת אם שליחת המייל נכשלה
            Console.WriteLine($"שגיאה בשליחת המייל ({formTypeName}): {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"פירוט נוסף: {ex.InnerException.Message}");
            }
        }
    }

}