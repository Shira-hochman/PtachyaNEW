// File: Bo/Services/FormService.cs

using Bo.Interfaces;
using Dto;
using Dal.Models;
using Dal.Repositories.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

// ❌ כל ה-using האלה מיותרים כאן ועלולים לגרום לבלבול/בעיות תאימות
// using NPOI.XWPF.UserModel; 
// using NPOI.OpenXmlFormats.Wordprocessing;
// using System.Drawing; 

public class FormService : IFormService
{
    private readonly IFormRepository _formRepo;
    private readonly IChildRepository _childRepo;

    public FormService(IFormRepository formRepo, IChildRepository childRepo)
    {
        _formRepo = formRepo;
        _childRepo = childRepo;
    }

    public async Task<byte[]> ProcessAndGenerateHealthDeclarationAsync(HealthDeclarationDto declarationDto)
    {
        try
        {
            // 1. שלב האימות
            var child = await _childRepo.GetByIdNumberAsync(declarationDto.ChildDetails.ChildId);
            if (child == null)
            {
                // ⭐️ זורק שגיאה ספציפית
                throw new ArgumentException($"Child with ID {declarationDto.ChildDetails.ChildId} not found.");
            }

            // 2. יצירת קובץ ה-PDF (החלק שקורס)
            // ⚠️ בדיקה קריטית: נתיב לקובץ התבנית (ודא ש-"Templates" קיימת ושהקובץ מועתק)
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "health_declaration_template.docx");

            // ⭐️ בדיקה נוספת: לוודא שהתבנית קיימת לפני שקוראים ל-Generator
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Template file not found at: {templatePath}. Check Copy to Output Directory setting.");
            }

            // WordTemplateGenerator.GenerateDocument מחזיר byte[] של PDF
            byte[] pdfBytes = WordTemplateGenerator.GenerateDocument(declarationDto, templatePath);

            // 3. שמירה בבסיס הנתונים
            var formEntity = new Form
            {
                ChildId = child.ChildId,
                FileContent = pdfBytes, // שומרים את ה-PDF
                SubmittedDate = DateTime.Now,
                ContentType = "application/pdf" // סוג התוכן שונה ל-PDF
            };

            await _formRepo.AddAsync(formEntity);

            // ⭐️ החזרת הבייטים של ה-PDF
            return pdfBytes;
        }
        catch (Exception ex)
        {
            // 💡 הדפסה לקונסול כדי לראות את השגיאה המדויקת של Aspose/NPOI
            Console.WriteLine($"Service Layer Fatal Error: {ex.Message}");

            // זורק מחדש כדי שה-Controller יתפוס (שגיאת 500)
            throw;
        }
    }
}