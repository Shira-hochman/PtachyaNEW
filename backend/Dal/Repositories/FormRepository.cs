using Dal.Models;
using Dal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Dal.Repositories;

public class FormRepository : IFormRepository
{
    private readonly PtachiyaContext _context;

    public FormRepository(PtachiyaContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Form entity)
    {
        // שמירת ה-Entity לבסיס הנתונים
        _context.Forms.Add(entity);
        await _context.SaveChangesAsync();
    }

    // ⭐️ מימוש: מציאת המפתח הראשי ChildId לפי IdNumber (תעודת הזהות)
    public async Task<int?> GetChildPkByIdNumberAsync(string idNumber)
    {
        var child = await _context.Children
            .Where(c => c.IdNumber == idNumber)
            .Select(c => (int?)c.ChildId) // ⬅️ מחזיר את ChildId (המפתח הראשי)
            .FirstOrDefaultAsync();

        return child;
    }


}