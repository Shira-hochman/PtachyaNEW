using System;

namespace Dto
{
    public class FormManageDto
    {
        public int FormId { get; set; }
        public string FormType { get; set; }
        public string Status { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public string FilePath { get; set; }

        // נתונים שטוחים של הילד (ללא קינון)
        public string ChildFirstName { get; set; }
        public string ChildLastName { get; set; }
        public string ChildIdNumber { get; set; }
    }
}