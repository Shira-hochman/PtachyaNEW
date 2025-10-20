namespace Dto
{
    public class ParentChildImportDto
    {
 
        

        // נתוני הילד
        public string FullName { get; set; } = string.Empty;
        public string IdNumber { get; set; } = string.Empty; // מפתח ל-Upsert ילד
        public DateTime ?BirthDate { get; set; } // שינוי ל-DateTime כי Excel נותן DateTime
        public string SchoolYear { get; set; } = string.Empty;
        public string FormLink { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty; // מפתח ל-Upsert הורה
        public string Phone { get; set; } = string.Empty;
        public string KindergartenId { get; set; } = string.Empty; // מפתח לאיתור הגן

    }
}