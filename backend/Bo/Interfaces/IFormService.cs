// File: Bo/Interfaces/IFormService.cs

using Dto;
using System.Threading.Tasks;

namespace Bo.Interfaces
{
    public interface IFormService
    {
        // ⭐️ תיקון שגיאה 2: מחזיר רק byte[]
        Task<byte[]> ProcessAndGenerateHealthDeclarationAsync(HealthDeclarationDto declarationDto);
    }
}