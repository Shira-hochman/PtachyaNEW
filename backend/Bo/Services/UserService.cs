using Bo.Interfaces;
using Dal.Converters;
using Dal.Models;
using Dal.Repositories.Interfaces;
using Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bo.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;

        public UserService(IUserRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<UserDto>> GetUserAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task AddUserAsync(UserDto dto)
        {
            var entity = UserConverter.ToUserEntity(dto); // שימוש בממיר
            await _repo.AddAsync(entity); // שולח Entity
        }

    }
}