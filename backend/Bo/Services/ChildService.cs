using Bo.Interfaces;
using Dal.Models;
using Dal.Repositories.Interfaces;
using Dal_Repository.ModelsConverters; 
using Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bo.Services
{
    public class ChildService : IChildService
    {
        private readonly IChildRepository _repo;

        public ChildService(IChildRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<ChildDto>> GetChildrenAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task AddChildAsync(ChildDto dto)
        {
            var entity = new Child
            {
                KindergartenId = dto.KindergartenId,
                IdNumber = dto.IdNumber,
                FullName = dto.FullName,
                SchoolYear = dto.SchoolYear,
                FormLink = dto.FormLink,
                Phone = dto.Phone,
                Email = dto.Email,
                BirthDate = dto.BirthDate,

            };

            await _repo.AddAsync(entity);
        }

        public async Task RemoveChildAsync(ChildDto dto)
        {
            if (dto.ChildId <= 0)
            {
                throw new ArgumentException("Child ID must be valid for deletion.");
            }
            await _repo.DeleteAsync(dto.ChildId);
        }
    }
}