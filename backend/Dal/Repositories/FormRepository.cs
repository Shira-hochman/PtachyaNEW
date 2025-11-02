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
        string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "health_declaration_template.docx");
        _context.Forms.Add(entity);
        await _context.SaveChangesAsync();
    }
}