using System.Collections.Generic;
using System.Threading.Tasks;
using Dal.Models;
using Dto;

namespace Dal.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<List<UserDto>> GetAllAsync();
        Task AddAsync(User entity);
        Task<User?> GetByIDAsync(int id);
        Task UpdateAsync(User user);
        Task<User?> GetByUsernameAsync(string username);
        Task DeleteAsync(int id);
    }
}