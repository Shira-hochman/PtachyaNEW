using Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bo.Interfaces
{
    public interface IKindergartenService
    {
        Task<List<KindergartenDto>> GetKindergartensAsync();
        Task AddKindergartenAsync(KindergartenDto dto);
        Task RemoveKindergartenAsync(int id); 
    }
}