using Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bo.Interfaces
{
   public  interface IUserService
    {
         Task AddUserAsync(UserDto dto);
         Task<List<UserDto>> GetUserAsync();
        Task<object> ValidateUserWithFeedbackAsync(string username, string password);

    }
}
