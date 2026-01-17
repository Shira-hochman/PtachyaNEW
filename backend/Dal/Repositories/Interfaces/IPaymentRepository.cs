using Dal.Models;
using Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dal.Repositories.Interfaces
{
    public interface IPaymentRepository
    {
        Task<List<PaymentDto>> GetAllAsync();
        Task<Payment?> GetByIDAsync(int id);
        Task AddAsync(Payment entity);
        Task UpdateAsync(Payment entity);
        Task DeleteAsync(int id);
        Task<List<Payment>> GetByChildIdAsync(int childId); // פונקציה שימושית להיסטוריית תשלומים של ילד
    }
}