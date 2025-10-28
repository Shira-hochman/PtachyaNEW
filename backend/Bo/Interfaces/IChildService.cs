using Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bo.Interfaces
{
    public interface IChildService
    {
        Task<List<ChildDto>> GetChildrenAsync();
        Task AddChildAsync(ChildDto dto);
        Task RemoveChildAsync(ChildDto dto);
        Task<string> VerifyChildIdentityAsync(string idNumber, DateTime birthDate);
    }
}