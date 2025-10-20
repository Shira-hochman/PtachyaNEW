using Dal.Converters;
using Dal.Models;
using Dal.Repositories.Interfaces;
using Dto;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dal.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly PtachiyaContext _context;

        public UserRepository(PtachiyaContext context)
        {
            _context = context;
        }

        public async Task<List<UserDto>> GetAllAsync()
        {
            var entities = await _context.Users.ToListAsync();
            return entities.Select(UserConverter.ToUserDto).ToList();
        }

        public async Task AddAsync(User entity)
        {
            _context.Users.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<User?> GetByIDAsync(string PasswordHash)
        {
            return await _context.Set<User>().FirstOrDefaultAsync(k => k.PasswordHash == PasswordHash);
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        
    }
}