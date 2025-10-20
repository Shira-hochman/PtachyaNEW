using Dal.Models;
using Dto;
using System.Collections.Generic;
using System.Linq;

namespace Dal.Converters
{
    // *** יש לוודא שהמחלקה היא public static כדי להשתמש בה בקלות ***
    public static class UserConverter
    {
        // המרה מיישות (Entity) ל-DTO
        public static UserDto ToUserDto(User user)
        {
            return new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                PasswordHash = user.PasswordHash
            };
        }

        // המרה מ-DTO ליישות (Entity)
        public static User ToUserEntity(UserDto dto)
        {
            return new User
            {
                UserId = dto.UserId,
                Username = dto.Username,
                PasswordHash = dto.PasswordHash
            };
        }

        // המרה מרשימת Entities ל-DTOs
        public static List<UserDto> ToUserDtoList(List<User> users)
        {
            return users.Select(ToUserDto).ToList();
        }

        // המרה מרשימת DTOs ל-Entities
        public static List<User> ToUserEntityList(List<UserDto> userDtos)
        {
            return userDtos.Select(ToUserEntity).ToList();
        }
    }
}
