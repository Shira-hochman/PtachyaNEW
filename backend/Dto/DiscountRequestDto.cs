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
        // ⭐️ הנתונים הראשיים של הטופס שנשלחו כ-JSON string תחת השם 'data'
        // חשוב: השם "data" כאן חייב להתאים ל-formData.append('data', ...) באנגולר
        [FromForm(Name = "data")]
        public string Data { get; set; } = null!;

        // ⭐️ הקבצים המצורפים (חייבים להתאים לשמות ששלחנו: lowIncomeFile וכו')
        public IFormFile? LowIncomeFile { get; set; }
        public IFormFile? SpecialEdFile { get; set; }
        public IFormFile? SocialWorkerFile { get; set; }

        // מתודת עזר פשוטה לאיסוף הקבצים
        public List<IFormFile> GetAttachments()
        {
            var files = new List<IFormFile>();
            // ⭐️ שימוש בבדיקה מפורשת של null לפני ההוספה ⭐️
            if (LowIncomeFile != null) files.Add(LowIncomeFile);
            if (SpecialEdFile != null) files.Add(SpecialEdFile);
            if (SocialWorkerFile != null) files.Add(SocialWorkerFile);
            return files;
        }

    }
}