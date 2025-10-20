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
        Task DeleteAsync(int childId);
    }
}