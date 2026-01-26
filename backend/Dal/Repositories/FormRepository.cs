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

    public async Task UpdateFormAsync(Form form)
    {
        _context.Forms.Update(form);
        await _context.SaveChangesAsync();
    }

    public async Task ApproveFormAsync(int formId)
    {
        var form = await _context.Forms.FindAsync(formId);
        if (form == null) return;

        form.Status = "Approved";

        if (form.FormType == "DISCOUNT_REQUEST")
        {
            var existingPayment = await _context.Payments
                .FirstOrDefaultAsync(p => p.ChildId == form.ChildId);

            if (existingPayment == null)
            {
                var newPayment = new Payment
                {
                    ChildId = form.ChildId,
                    Amount = 0,
                    Status = "Paid",
                    PaymentDate = DateTime.Now
                };
                _context.Payments.Add(newPayment);
                await _context.SaveChangesAsync(); // שמירה כדי לקבל ID לתשלום

                // עדכון ישיר של הילד
                var child = await _context.Children.FindAsync(form.ChildId);
                if (child != null)
                {
                    child.PaymentId = newPayment.PaymentId;
                    _context.Children.Update(child); // וידוא עדכון
                }
            }
            else
            {
                existingPayment.Status = "Paid";
                var child = await _context.Children.FindAsync(form.ChildId);
                if (child != null) child.PaymentId = existingPayment.PaymentId;
            }
        }
        await _context.SaveChangesAsync(); // שמירה סופית של כל השינויים
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