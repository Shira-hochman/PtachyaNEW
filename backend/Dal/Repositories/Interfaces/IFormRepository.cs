using Dal.Models;
using System.Threading.Tasks;

namespace Dal.Repositories.Interfaces;

public interface IFormRepository
{
    // משמש לשמירת הטופס החדש שנוצר (כולל קובץ Word)
    Task AddAsync(Form entity);

    // ⭐️ הצהרה חדשה: פותרת את שגיאת הקומפילציה ב-FormService
    Task UpdateChildFormLinkAsync(int childId, string formLink);
}