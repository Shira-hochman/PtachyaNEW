using Dal.Models;
using System.Threading.Tasks;

namespace Dal.Repositories.Interfaces;

public interface IFormRepository
{
    // משמש לשמירת הטופס החדש שנוצר (כולל קובץ Word)
    Task AddAsync(Form entity);
    // ⭐️ הוספה: פונקציה למציאת המפתח הראשי הפנימי (PK) לפי תעודת זהות (IdNumber)
    Task<int?> GetChildPkByIdNumberAsync(string idNumber);

}