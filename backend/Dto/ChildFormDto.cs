using System;

namespace Dto
{
    public class ChildFormDto
    {
        public int FormId { get; set; }
        public string FormType { get; set; }
        public string Status { get; set; } // 👈 הוספתי את זה! חובה!
        public string FileName { get; set; }
        public string DownloadUrl { get; set; }
        public DateTime UploadDate { get; set; }
        public List<string> AttachmentUrls { get; set; } = new List<string>();
    }
}