// WordTemplateGenerator.cs
using Dto;
using Aspose.Words;
using Aspose.Words.Drawing;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Aspose.Words.Replacing;

// ⭐️ זוהי המחלקה שמבצעת את כל העיבוד באמצעות Aspose.Words
public static class WordTemplateGenerator
{
    // פונקציית עזר לפורמט תאריך נקי (דרוש בגלל שימוש ב-DateTime ב-DTO)
    private static string FormatDate(DateTime date)
    {
        // אם התאריך לא תקין או ריק, מחזירים מחרוזת ריקה.
        if (date == default(DateTime) || date.Year < 1900) return string.Empty;
        return date.ToString("dd/MM/yyyy");
    }

    // הגדרת כל המיפויים לטקסט מילוי (Mail Merge Fields)
    private static readonly Dictionary<string, Func<HealthDeclarationDto, string>> TextMapping = new Dictionary<string, Func<HealthDeclarationDto, string>>
    {
        // שדות תאריך
        {"FormDate", dto => FormatDate(dto.FormDate)},
        {"ChildDob", dto => FormatDate(dto.ChildDetails.ChildDob)},
        
        // שדות טקסט/מספר
        {"ChildFirstName", dto => dto.ChildDetails.ChildFirstName ?? string.Empty},
        {"ChildLastName", dto => dto.ChildDetails.ChildLastName ?? string.Empty},
        {"ChildId", dto => dto.ChildDetails.ChildId ?? string.Empty},
        {"ChildAddress", dto => dto.ChildDetails.ChildAddress ?? string.Empty},

        {"ProgramProvider", dto => dto.ProgramProvider ?? string.Empty},
        {"ProgramFramework", dto => dto.ProgramFramework ?? string.Empty},

        {"FacilityName", dto => dto.FacilityDetails.FacilityName ?? string.Empty},
        {"FacilityOwnership", dto => dto.FacilityDetails.FacilityOwnership ?? string.Empty},
        {"FacilityManagerName", dto => dto.FacilityDetails.FacilityManagerName ?? string.Empty},
        {"FacilityAddress", dto => dto.FacilityDetails.FacilityAddress ?? string.Empty},
        {"FacilityPhone", dto => dto.FacilityDetails.FacilityPhone ?? string.Empty},

        {"MonthlySelfParticipation", dto => dto.MonthlySelfParticipation.ToString("N2") + " ₪"},

        {"Parent1Name", dto => dto.Parent1.Name ?? string.Empty},
        {"Parent1Phone", dto => dto.Parent1.Phone ?? string.Empty},
        {"Parent2Name", dto => dto.Parent2.Name ?? string.Empty},
        {"Parent2Phone", dto => dto.Parent2.Phone ?? string.Empty},

        {"NoOtherProgramDeclaration", dto => dto.NoOtherProgramDeclaration ? "כן" : "לא"}
    };

    // ⭐️ פונקציה ראשית: מחזירה מערך בתים של קובץ PDF
    public static byte[] GenerateDocument(HealthDeclarationDto dto, string templatePath)
    {
        if (!File.Exists(templatePath))
            throw new FileNotFoundException($"Template file not found at: {templatePath}");

        var doc = new Document(templatePath);

        // 1. מילוי נתונים טקסטואליים
        doc.MailMerge.Execute(
            TextMapping.Keys.ToArray(),
            TextMapping.Values.Select(f => (object)f(dto)).ToArray()
        );

        // 2. טיפול והחלפת חתימות (תמונות Base64)
        InsertSignature(doc, "Parent1Signature", dto.Parent1.Signature ?? string.Empty, true);
        InsertSignature(doc, "Parent2Signature", dto.Parent2.Signature ?? string.Empty, false);

        // 3. שמירה כ-PDF לזרם בזיכרון
        using (var pdfStream = new MemoryStream())
        {
            doc.Save(pdfStream, SaveFormat.Pdf);
            return pdfStream.ToArray();
        }
    }

    // ⭐️ פונקציית עזר להחלפת Placeholder של טקסט בתמונת Base64
    private static void InsertSignature(Document doc, string placeholderName, string base64Data, bool isRequired)
    {
        string placeholder = $"<<{placeholderName}>>";

        if (string.IsNullOrEmpty(base64Data))
        {
            string replacementText = isRequired ? "(חתימה חסרה - חובה)" : "(לא נחתם)";
            doc.Range.Replace(placeholder, replacementText, new FindReplaceOptions());
            return;
        }

        try
        {
            // 1. הסרת קידומת DataURL ושמירת הבתים
            var base64WithoutPrefix = base64Data.Contains(',') ? base64Data.Substring(base64Data.IndexOf(',') + 1) : base64Data;
            byte[] imageBytes = Convert.FromBase64String(base64WithoutPrefix);

            // 2. יצירת מופע של ה-Callback והחלפה
            ImageReplacingCallback callback = new ImageReplacingCallback(doc, imageBytes);

            FindReplaceOptions options = new FindReplaceOptions();
            options.ReplacingCallback = callback;

            // ⭐️ הפונקציה doc.Range.Replace עכשיו מקבלת את ה-Callback התקין
            doc.Range.Replace(placeholder, string.Empty, options);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error inserting signature {placeholderName}: {ex.Message}");
            doc.Range.Replace(placeholder, "(שגיאת חתימה)", new FindReplaceOptions());
        }
    }

    // ⭐️ מחלקה פנימית המממשת IReplacingCallback כדי לפתור את שגיאת ה-Delegate/Lambda
    private class ImageReplacingCallback : IReplacingCallback
    {
        private readonly Document _document;
        private readonly byte[] _imageBytes;
        private const double SignatureWidth = 180;
        private const double SignatureHeight = 60;

        public ImageReplacingCallback(Document document, byte[] imageBytes)
        {
            _document = document;
            _imageBytes = imageBytes;
        }

        ReplaceAction IReplacingCallback.Replacing(ReplacingArgs args)
        {
            DocumentBuilder builder = new DocumentBuilder(_document);
            builder.MoveTo(args.MatchNode);

            using (var imageStream = new MemoryStream(_imageBytes))
            {
                Shape signatureShape = builder.InsertImage(imageStream);
                signatureShape.Width = SignatureWidth;
                signatureShape.Height = SignatureHeight;
            }

            // הסרת הצומת המקורית של ה-Placeholder
            args.MatchNode.Remove();
            return ReplaceAction.Skip;
        }
    }
}
