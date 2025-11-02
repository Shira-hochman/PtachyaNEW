// FormService.cs

using Bo.Interfaces;
using Dto;
using Dal.Repositories.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

public class FormService : IFormService
{
    private readonly IFormRepository _formRepository;
    private const string TemplateFileName = "health_declaration_template.docx";

    public FormService(IFormRepository formRepository)
    {
        _formRepository = formRepository;
    }

    public async Task<byte[]> ProcessAndGenerateHealthDeclarationAsync(HealthDeclarationDto declarationDto)
    {
        // 1. קביעת נתיב התבנית (בתיקיית Templates ב-Root)
        string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", TemplateFileName);

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template file not found: {templatePath}. Make sure '{TemplateFileName}' is in the 'Templates' folder.");
        }

        // 2. יצירת קובץ ה-PDF
        byte[] pdfBytes = WordTemplateGenerator.GenerateDocument(declarationDto, templatePath);

        // 3. שמירת הנתונים לבסיס הנתונים (יש לממש מיפוי DTO ל-Entity)
        // לדוגמה:
        // var entity = DtoToEntityMapper.Map(declarationDto);
        // await _formRepository.AddAsync(entity); 

        // ⭐️ מחזירים את מערך הבתים של קובץ ה-PDF ל-Controller
        return pdfBytes;
    }
}
