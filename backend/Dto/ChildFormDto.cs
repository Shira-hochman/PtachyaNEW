using System;

namespace Dto
{
    public class ChildFormDto
    {
        public int FormId { get; set; }
        public string FormType { get; set; } // סוג הטופס (למשל HEALTH_DECLARATION)
        public string FileName { get; set; } // שם הקובץ לתצוגה
        public string DownloadUrl { get; set; } // הלינק המלא להורדה
        public DateTime UploadDate { get; set; }
        public List<string> AttachmentUrls { get; set; } = new List<string>();
    }
}