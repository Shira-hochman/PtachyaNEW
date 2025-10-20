using Dal.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dal.Repositories.Interfaces
{
    

    public interface IKindergartenRepository
    {
        Task<IEnumerable<Kindergarten>> GetAllAsync();
        Task<Kindergarten?> GetByIdAsync(int id);
        Task AddAsync(Kindergarten kindergarten); // מקבל ישות
        Task UpdateAsync(Kindergarten kindergarten); // מקבל ישות
        Task DeleteAsync(int id);

        Task<Kindergarten?> GetByCodeAsync(string code);

    }
}