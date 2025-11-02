using Dto;
using NPOI.XWPF.UserModel;
using System.IO;
using System;
using System.Collections.Generic;
using Aspose.Words;
using Aspose.Words.Saving;
using Aspose.Words.Loading;
using System.Text;
using System.Linq; // נדרש עבור LINQ ב-ReplaceTextInParagraphs

public static class WordTemplateGenerator
{
    // ⭐️⭐️ הפונקציה הראשית: מחזירה PDF תקין
    public static byte[] GenerateDocument(HealthDeclarationDto dto, string templatePath)
    {
        try
        {
            byte[] docxBytes = FillDocxTemplate(dto, templatePath);

            using (MemoryStream docxStream = new MemoryStream(docxBytes))
            {
                Aspose.Words.Document doc = new Aspose.Words.Document(docxStream);

                // 2. המרה ל-HTML Stream וטיפול ב-Base64
                using (MemoryStream htmlStream = new MemoryStream())
                {
                    // ⭐️⭐️ תיקון שגיאת התמונות: יצירת אובייקט HtmlSaveOptions
                    HtmlSaveOptions htmlSaveOptions = new HtmlSaveOptions();
                    // הגדרה זו חיונית כדי למנוע מ-Aspose לכתוב קובצי תמונה לדיסק זמני.
                    // במקום זאת, הוא משתמש ב-Base64 בתוך ה-HTML.
                    htmlSaveOptions.ExportImagesAsBase64 = true;

                    // שומר את ה-DOCX הפנימי כ-HTML
                    doc.Save(htmlStream, htmlSaveOptions); // ⭐️ משתמשים ב-Options החדש

                    htmlStream.Position = 0;
                    string htmlContent = new StreamReader(htmlStream, Encoding.UTF8).ReadToEnd();

                    // ⭐️ שלב החלפת החתימה בתוך קוד ה-HTML (הופך טקסט לתגית <img>)
                    htmlContent = ReplaceSignaturesInHtml(htmlContent, dto);

                    // 3. יצירת PDF מ-HTML הנקי
                    using (MemoryStream finalHtmlStream = new MemoryStream(Encoding.UTF8.GetBytes(htmlContent)))
                    {
                        LoadOptions loadOptions = new LoadOptions
                        {
                            LoadFormat = LoadFormat.Html
                        };
                        Aspose.Words.Document finalDoc = new Aspose.Words.Document(finalHtmlStream, loadOptions);

                        PdfSaveOptions saveOptions = new PdfSaveOptions();
                        saveOptions.Compliance = PdfCompliance.Pdf17;

                        using (MemoryStream pdfStream = new MemoryStream())
                        {
                            // ⭐️ השמירה ל-PDF אמורה כעת לעבוד ללא גישה לדיסק
                            finalDoc.Save(pdfStream, saveOptions);
                            return pdfStream.ToArray();
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ FATAL ERROR in Generator: {ex.Message}");
            throw new InvalidOperationException($"Failed to generate PDF: {ex.Message}", ex);
        }
    }

    // -----------------------------------------------------------------------

    private static string ReplaceSignaturesInHtml(string htmlContent, HealthDeclarationDto dto)
    {
        // ⭐️ החלפת מחרוזת Base64 לתגית <img> שנתמכת ב-Aspose
        string parent1SignatureHtml = string.IsNullOrWhiteSpace(dto.Parent1.Signature)
            ? "<span style='font-style: italic;'>(לא נחתם)</span>"
            : $"<img src=\"{dto.Parent1.Signature}\" style=\"width:180px; height:60px; border: 1px solid #ccc;\"/>";

        htmlContent = htmlContent.Replace("<<Parent1Signature>>", parent1SignatureHtml);

        string parent2SignatureHtml = string.IsNullOrWhiteSpace(dto.Parent2.Signature)
            ? "<span style='font-style: italic;'>(לא נחתם)</span>"
            : $"<img src=\"{dto.Parent2.Signature}\" style=\"width:180px; height:60px; border: 1px solid #ccc;\"/>";

        htmlContent = htmlContent.Replace("<<Parent2Signature>>", parent2SignatureHtml);

        return htmlContent;
    }

    // -----------------------------------------------------------------------

    private static byte[] FillDocxTemplate(HealthDeclarationDto dto, string templatePath)
    {
        // ⭐️ NPOI ממלא את הטקסט בלבד
        if (!File.Exists(templatePath))
            throw new FileNotFoundException($"Template file not found at: {templatePath}");

        using (FileStream fs = new FileStream(templatePath, FileMode.Open, FileAccess.Read))
        {
            XWPFDocument document = new XWPFDocument(fs);
            ReplacePlaceholderText(document, dto);

            using (MemoryStream ms = new MemoryStream())
            {
                document.Write(ms);
                return ms.ToArray();
            }
        }
    }

    // -----------------------------------------------------------------------

    private static void ReplacePlaceholderText(XWPFDocument document, HealthDeclarationDto dto)
    {
        // ⭐️ החתימות מועברות כטקסט רגיל (Base64)
        var replacements = new Dictionary<string, string>
        {
            { "<<FormDate>>", dto.FormDate.ToShortDateString() },
            { "<<FacilityName>>", dto.FacilityDetails.FacilityName ?? "" },
            { "<<FirstName>>", dto.ChildDetails.ChildFirstName ?? "" },
            { "<<LastName>>", dto.ChildDetails.ChildLastName ?? "" },
            { "<<IDNumber>>", dto.ChildDetails.ChildId ?? "" },
            { "<<DateOfBirth>>", dto.ChildDetails.ChildDob.ToShortDateString() },
            { "<<Address>>", dto.ChildDetails.ChildAddress ?? "" },
            { "<<ProgramProvider>>", dto.ProgramProvider ?? "" },
            { "<<ManagerName>>", dto.FacilityDetails.FacilityManagerName ?? "" },
            { "<<FacilityAddress>>", dto.FacilityDetails.FacilityAddress ?? "" },
            { "<<FacilityPhone>>", dto.FacilityDetails.FacilityPhone ?? "" },
            { "<<SelfParticipation>>", dto.MonthlySelfParticipation.ToString("N2") + " ₪" },
            { "<<Parent1Name>>", dto.Parent1.Name ?? "" },
            { "<<Parent1Phone>>", dto.Parent1.Phone ?? "" },
            { "<<Parent2Name>>", dto.Parent2.Name ?? "" },
            { "<<Parent2Phone>>", dto.Parent2.Phone ?? "" },
            
            // ⭐️⭐️ החתימות מועברות כטקסט Base64 לתוך ה-DOCX
            { "<<Parent1Signature>>", dto.Parent1.Signature ?? "" },
            { "<<Parent2Signature>>", dto.Parent2.Signature ?? "" },
        };

        ReplaceTextInParagraphs(document.Paragraphs, replacements);

        foreach (var table in document.Tables)
        {
            foreach (var row in table.Rows)
            {
                foreach (var cell in row.GetTableCells())
                    ReplaceTextInParagraphs(cell.Paragraphs, replacements);
            }
        }
    }

    // -----------------------------------------------------------------------

    private static void ReplaceTextInParagraphs(IEnumerable<XWPFParagraph> paragraphs, Dictionary<string, string> replacements)
    {
        // ⭐️ לוגיקת החלפת הטקסט הכללית של NPOI
        foreach (var paragraph in paragraphs)
        {
            string text = paragraph.Text;
            if (string.IsNullOrWhiteSpace(text)) continue;

            bool changed = false;
            foreach (var pair in replacements)
            {
                if (text.Contains(pair.Key))
                {
                    text = text.Replace(pair.Key, pair.Value ?? "");
                    changed = true;
                }
            }

            if (changed)
            {
                for (int i = paragraph.Runs.Count - 1; i >= 0; i--)
                    paragraph.RemoveRun(i);

                var run = paragraph.CreateRun();
                run.SetText(text);
            }
        }
    }
}