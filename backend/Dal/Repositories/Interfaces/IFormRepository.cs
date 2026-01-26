using Dal.Models;
using Dto;
using System.Threading.Tasks;

namespace Dal.Repositories.Interfaces;

public interface IFormRepository
{
    // משמש לשמירת הטופס החדש שנוצר (כולל קובץ Word)
    Task AddAsync(Form entity);
    // ⭐️ הוספה: פונקציה למציאת המפתח הראשי הפנימי (PK) לפי תעודת זהות (IdNumber)
    Task<int?> GetChildPkByIdNumberAsync(string idNumber);
    // ⭐️ הוספות חדשות לתמיכה בדשבורד וניהול טפסים
    Task ApproveFormAsync(int formId);
    Task UpdateFormAsync(Form form);
    Task<List<Form>> GetPendingFormsAsync();
    Task<DashboardStatsDto> GetDashboardStatsAsync();
    Task    <List<Form>> GetApprovedFormsAsync();
    Task<List<Dal.Models.Form>> GetFormsByChildIdAsync(int childId);
    Task<string> GetChildEmailByIdAsync(int childId);


}