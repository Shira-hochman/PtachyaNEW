//using Dal.Models;
//using Dto;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace Dal.Repositories.Interfaces
//{
//    public interface IParentRepository
//    {
//        Task<Parent?> GetByEmailAsync(string email);
//        Task UpdateAsync(Parent parent);

//        // *** תיקון CS1503: מקבל Entity כדי להתאים ל-Repository AddAsync ***
//        Task AddAsync(Parent entity);

//        Task<List<ParentDto>> GetAllAsync();
//        Task DeleteAsync(int parentId);
//    }
//}