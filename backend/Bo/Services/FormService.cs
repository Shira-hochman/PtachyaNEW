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
        string signatureHtml = ConvertSignatureToHtml(declarationDto.Parent1.Signature);

        // החלפת נתונים
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
            .Replace("{{ParentName}}", declarationDto.Parent1.Name)
            .Replace("{{SignatureImageTag}}", signatureHtml);

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
    public async Task<byte[]> ProcessAndGenerateDiscountRequestAsync(DiscountRequestDto requestDto, string uploadedPaths)
    {
        string idNumber = requestDto.StudentDetails.StudentId;
        int? actualChildPK = await _formRepository.GetChildPkByIdNumberAsync(idNumber);

        if (!actualChildPK.HasValue)
            throw new ArgumentException($"Child with ID {idNumber} not found.");

        int childPK = actualChildPK.Value;

        string htmlContent = await LoadTemplateAsync(DiscountTemplateHtml);
        string signatureHtml = ConvertSignatureToHtml(requestDto.ParentSignature);
        string childrenRows = GenerateChildrenTableRows(requestDto.ChildrenInCustody);

        string xMark = "X";
        string emptyMark = "";

        var reasons = requestDto.DiscountReasons;
        var income = requestDto.LowIncomeDetails;
        var student = requestDto.StudentDetails;

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

        byte[] pdfBytes = await GeneratePdfFromHtmlAsync(htmlContent);
        await SaveFormRecordAsync(childPK, "DISCOUNT_REQUEST", pdfBytes, uploadedPaths);

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
            return new ChildFormDto
            {
                FormId = f.FormId,
                FormType = f.FormType,
                FileName = fileName,
                DownloadUrl = $"{baseUrl}api/Form/Download?container={PermanentFormsFolder}&fileName={fileName}",
                UploadDate = f.SubmittedDate ?? DateTime.MinValue
            };
        }).ToList();
    }

    // ---------------------------------------------------------
    // 📧 פונקציית שליחת מייל
    // ---------------------------------------------------------
    private async Task SendPdfByEmailAsync(string parentEmail, string childName, byte[] pdfBytes)
    {
        try
        {
            string senderEmail = "syrhhwkmn17@gmail.com";
            string senderPassword = "icwxylkjdkstkown"; // כאן יש לשים את קוד ה-16 תווים מ-Google

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, "מערכת ניהול טפסים"),
                Subject = $"הצהרת בריאות חתומה - {childName}",
                Body = $"שלום רב,\nמצורפת בזאת הצהרת הבריאות עבור {childName}.\nבברכה.",
                IsBodyHtml = false,
            };

            mailMessage.To.Add(parentEmail);

            using (var ms = new MemoryStream(pdfBytes))
            {
                var attachment = new Attachment(ms, $"Health_Declaration_{childName}.pdf", "application/pdf");
                mailMessage.Attachments.Add(attachment);
                await smtpClient.SendMailAsync(mailMessage);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"שגיאה בשליחת המייל: {ex.Message}");
            // לא נזרוק שגיאה כדי לא לעצור את כל התהליך אם רק המייל נכשל
        }
    }
}