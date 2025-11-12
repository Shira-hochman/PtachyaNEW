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

    // ⭐️ פונקציה חדשה: עדכון קישור לטופס בטבלת Child
    public async Task UpdateChildFormLinkAsync(int childId, string formLink)
    {
        // 1. המרת ה-int (המכיל את הת.ז.) למחרוזת, מכיוון שהשדה IdNumber ב-DB הוא string.
        string identityNumberString = childId.ToString();

        // 2. מחפשים את הילד לפי תעודת זהות (IdNumber)
        // עכשיו החיפוש הוא: c.IdNumber == identityNumberString
        var childToUpdate = await _context.Children.FirstOrDefaultAsync(c => c.IdNumber == identityNumberString);

        if (childToUpdate != null)
        {
            // 3. פשוט מעדכנים את השדה (Entity Framework עוקב אחרי השינוי)
            childToUpdate.FormLink = formLink;

            // 4. שולח את העדכון (UPDATE) למסד הנתונים
            await _context.SaveChangesAsync();
            Console.WriteLine($"Link saved successfully for child T.Z.: {identityNumberString} with link: {formLink}");
        }
        else
        {
            // 5. אם הילד לא נמצא, זורקים חריגה מפורשת
            throw new InvalidOperationException($"Child with T.Z. {identityNumberString} not found in database! Cannot save form link.");
        }
    }

}