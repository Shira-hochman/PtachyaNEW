using Dal.Models;
using Dal.Repositories.Interfaces;
using Dto; // ודאי שקיים
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
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
        _context.Forms.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<int?> GetChildPkByIdNumberAsync(string idNumber)
    {
        return await _context.Children
            .Where(c => c.IdNumber == idNumber)
            .Select(c => (int?)c.ChildId)
            .FirstOrDefaultAsync();
    }

    public async Task ApproveFormAsync(int formId)
    {
        // 1. שליפת הטופס
        var form = await _context.Forms.FindAsync(formId);

        if (form != null)
        {
            // 2. עדכון סטטוס הטופס
            form.Status = "Approved";

            // 3. ⭐️ לוגיקה חדשה: אם זה טופס הנחה, ניצור תשלום "מוסדר" לילד ⭐️
            if (form.FormType == "DISCOUNT_REQUEST")
            {
                // בדיקה אם כבר קיים תשלום כדי לא ליצור כפילויות
                var existingPayment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.ChildId == form.ChildId);

                if (existingPayment == null)
                {
                    // יצירת תשלום חדש
                    var newPayment = new Payment
                    {
                        ChildId = form.ChildId,
                        Amount = 0, // או הסכום אחרי הנחה, אם יש לך אותו
                        Status = "Paid", // סטטוס מוסדר
                        PaymentDate = DateTime.Now
                    };

                    _context.Payments.Add(newPayment);
                    await _context.SaveChangesAsync(); // שמירה כדי לקבל PaymentId

                    // עדכון הילד עם ה-PaymentId החדש (כדי שיופיע ירוק בטבלה)
                    var child = await _context.Children.FindAsync(form.ChildId);
                    if (child != null)
                    {
                        child.PaymentId = newPayment.PaymentId; // מניח שיש שדה כזה ב-Child לפי ה-DTO
                    }
                }
                else
                {
                    // אם כבר קיים תשלום, רק נעדכן אותו למשולם
                    existingPayment.Status = "Paid";
                    existingPayment.PaymentDate = DateTime.Now;

                    var child = await _context.Children.FindAsync(form.ChildId);
                    if (child != null && child.PaymentId == null)
                    {
                        child.PaymentId = existingPayment.PaymentId;
                    }
                }
            }

            // 4. שמירת כל השינויים (טופס + תשלום + ילד)
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Form>> GetPendingFormsAsync()
    {
        return await _context.Forms
            .Include(f => f.Child)
            .Where(f => f.Status == "Pending")
            .OrderByDescending(f => f.SubmittedDate)
            .ToListAsync();
    }

    public async Task<List<Form>> GetApprovedFormsAsync()
    {
        return await _context.Forms
            .Include(f => f.Child)
            .Where(f => f.Status == "Approved")
            .OrderByDescending(f => f.SubmittedDate)
            .Take(50)
            .ToListAsync();
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        return new DashboardStatsDto
        {
            TotalChildren = await _context.Children.CountAsync(),
            ActiveGardens = await _context.Kindergartens.CountAsync(),
            PendingForms = await _context.Forms.CountAsync(f => f.Status == "Pending"),
            UnpaidPayments = await _context.Payments.CountAsync(p => p.Status != "Paid")
        };
    }
    public async Task<List<Form>> GetFormsByChildIdAsync(int childId)
    {
        return await _context.Forms
            .Where(f => f.ChildId == childId)
            .OrderByDescending(f => f.SubmittedDate) // הכי חדש למעלה
            .ToListAsync();
    }
    public async Task<string> GetChildEmailByIdAsync(int childId)
    {
        // אנחנו ניגשים לטבלת הילדים ושולפים רק את שדה המייל לפי ה-PK
        return await _context.Children
            .Where(c => c.ChildId == childId) // וודאי שזה שם השדה של ה-ID ב-DB
            .Select(c => c.Email)            // וודאי שזה שם שדה המייל ב-DB
            .FirstOrDefaultAsync();
    }


}