using System;
using System.Collections.Generic;

namespace Dal.Models;

public partial class Form
{
    public int FormId { get; set; }

    public int ChildId { get; set; }

    // 🛑 הוסר: public string FormLink { get; set; } = null!;

    // ⭐️ הוספה: שדה לשמירת תוכן הקובץ הבינארי
    public byte[]? FileContent { get; set; }

    // ⭐️ הוספה: סוג הקובץ (MIME Type)
    public string ContentType { get; set; } = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    public DateTime? SubmittedDate { get; set; }

    public virtual Child Child { get; set; } = null!;
}