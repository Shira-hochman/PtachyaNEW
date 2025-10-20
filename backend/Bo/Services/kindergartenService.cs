using Dal.Repositories.Interfaces;
using Dto;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bo.Interfaces;
using Dal.Converters; 

namespace Bo.Services
{
    public class KindergartenService : IKindergartenService
    {
        private readonly IKindergartenRepository _repo;

        public KindergartenService(IKindergartenRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<KindergartenDto>> GetKindergartensAsync()
        {
            
            var entities = await _repo.GetAllAsync();

            return KindergartenConverter.TokindergartenDtoList(entities);
        }

        public async Task AddKindergartenAsync(KindergartenDto dto)
        {
            var entity = KindergartenConverter.TokindergartenEntity(dto);

            // 2. קריאה ל-Repository
            await _repo.AddAsync(entity);
        }

        public async Task RemoveKindergartenAsync(int id)
        {
            // מחיקה לפי Id
            await _repo.DeleteAsync(id);
        }

  }
}