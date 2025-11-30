// Dal.Models.Form.cs

using System;
using System.Collections.Generic;

namespace Dal.Models;

public partial class Form
{
    public int FormId { get; set; }

    public int ChildId { get; set; }

    // ⭐️ שדה חדש: הגדרת סוג הטופס
    public string FormType { get; set; } = null!;

    // ⭐️ שדה חדש: הקישור/נתיב של קובץ ה-PDF הראשי
    public string? FilePath { get; set; }

    // ⭐️ שדה חדש: נתיבי הקבצים המצורפים (CSV)
    public string? AttachmentPaths { get; set; }

    // ⭐️ שדה חדש: סטטוס הטופס (Pending / Approved / Rejected)
    public string Status { get; set; } = "Pending";


    public string? ContentType { get; set; } = "application/pdf";

    public DateTime? SubmittedDate { get; set; }

    public virtual Child Child { get; set; } = null!;
}