using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dto
{
    // ====================================================================
    // DTOs עבור טופס בקשת הנחה (Payment Form)
    // ====================================================================

    public class DiscountRequestDto
    {
        // נתונים ראשוניים של מגיש הבקשה
        public string DeclarantName { get; set; } = null!;
        public string DeclarantId { get; set; } = null!;
        public string MaritalStatus { get; set; } = null!;

        // נתוני ילדים בחזקת ההורה (FormArray)
        public int ChildrenCount { get; set; }
        public List<ChildInCustodyDto> ChildrenInCustody { get; set; } = new List<ChildInCustodyDto>();

        // קבוצות נתונים (Nested FormGroups)
        public StudentDetailsDto StudentDetails { get; set; } = new StudentDetailsDto();
        public DiscountReasonsDto DiscountReasons { get; set; } = new DiscountReasonsDto();
        public LowIncomeDetailsDto LowIncomeDetails { get; set; } = new LowIncomeDetailsDto();
        public RequiredDocumentsDto RequiredDocuments { get; set; } = new RequiredDocumentsDto();

        // שדות אחרונים
        public string Reasoning { get; set; } = null!;
        public string ParentSignature { get; set; } = null!; // חתימה כ-Base64
        public DateTime FormDate { get; set; } // התאריך הדיגיטלי
    }

    public class StudentDetailsDto
    {
        public string StudentName { get; set; } = null!;
        public string StudentId { get; set; } = null!;
        public string Kindergarten { get; set; } = null!;
        public string City { get; set; } = null!;
    }

    // יישום ה-FormArray של הילדים הנוספים
    public class ChildInCustodyDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Id { get; set; } // ת"ז של הילד הנוסף
    }

    public class DiscountReasonsDto
    {
        public bool LowIncome { get; set; }
        public bool OtherChildSpecialEd { get; set; }
        public bool SocialWorkerRec { get; set; }
    }

    public class LowIncomeDetailsDto
    {
        public string? Spouse1Status { get; set; }
        public decimal? Spouse1AvgMonthlyIncome { get; set; }
        public decimal? Spouse1Total3Months { get; set; }
        public string? Spouse2Status { get; set; }
        public decimal? Spouse2AvgMonthlyIncome { get; set; }
        public decimal? Spouse2Total3Months { get; set; }
    }

    // שמות הקבצים שהועלו (נשלחים כשם הקובץ או מחרוזת ריקה)
    public class RequiredDocumentsDto
    {
        public string? LowIncomeDocsUploaded { get; set; }
        public string? OtherSpecialEdDocsUploaded { get; set; }
        public string? SocialWorkerDocsUploaded { get; set; }
    }
    public class DiscountRequestSubmissionDto
    {
        [FromForm(Name = "data")]
        public string Data { get; set; } = null!;

        // שינוי לרשימות כדי לקלוט מספר קבצים לכל קטגוריה
        public List<IFormFile>? LowIncomeDocsUploaded { get; set; }
        public List<IFormFile>? OtherSpecialEdDocsUploaded { get; set; }
        public List<IFormFile>? SocialWorkerDocsUploaded { get; set; }

        public List<IFormFile> GetAttachments()
        {
            var files = new List<IFormFile>();

            // הוספת כל הקבצים מכל הרשימות לרשימה אחת עבור הטיפול ב-Controller
            if (LowIncomeDocsUploaded != null) files.AddRange(LowIncomeDocsUploaded);
            if (OtherSpecialEdDocsUploaded != null) files.AddRange(OtherSpecialEdDocsUploaded);
            if (SocialWorkerDocsUploaded != null) files.AddRange(SocialWorkerDocsUploaded);

            return files;
        }
    }
}