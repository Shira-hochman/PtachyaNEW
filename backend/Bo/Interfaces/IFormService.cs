using Dal.Models;
using Dto;
using System.Threading.Tasks;

namespace Bo.Interfaces
{
    public interface IFormService
    {
        // ⭐️ מחזירה Task<byte[]> של קובץ PDF
        Task<byte[]> ProcessAndGenerateHealthDeclarationAsync(HealthDeclarationDto declarationDto);

        Task<byte[]> ProcessAndGenerateDiscountRequestAsync(DiscountRequestDto requestDto, string uploadedPaths);
        // ⭐️ הוספות חדשות שנדרשות על ידי הקונטרולר
        Task ApproveFormAsync(int formId);
        Task<List<Form>> GetPendingFormsAsync();
        Task<List<Form>> GetApprovedFormsAsync();
    
        Task<List<ChildFormDto>> GetFormsByIdNumberAsync(string idNumber);
    }
}