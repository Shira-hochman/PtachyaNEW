using Dal.Models;
using Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dal.Repositories.Interfaces
{
    public interface IChildRepository
    {
        Task<List<ChildDto>> GetAllAsync();
        Task AddAsync(Child entity);

        Task<Child?> GetByIdNumberAsync(string idNumber);

        Task UpdateAsync(Child child);
       
        Task<Child?> GetByIdAsync(int id); // נוסיף את זה כדי לשלוף לפי ID פנימי ולא רק לפי ת.ז
        Task DeleteAsync(int childId);
        Task<PagedResult<ChildDto>> GetPagedAsync(
    int page,
    int pageSize,
    string? searchTerm,
    int? kindergartenId
);

    }
}