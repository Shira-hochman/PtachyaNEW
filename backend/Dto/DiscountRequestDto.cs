using System;
using System.Collections.Generic;

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

}