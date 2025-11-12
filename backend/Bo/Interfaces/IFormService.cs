using Dto;
using System.Threading.Tasks;

namespace Bo.Interfaces
{
    public interface IFormService
    {
        // ⭐️ מחזירה Task<byte[]> של קובץ PDF
        Task<byte[]> ProcessAndGenerateHealthDeclarationAsync(HealthDeclarationDto declarationDto);
    }
}