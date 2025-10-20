// דוגמה לממשק IImportService - בהנחה שהוא קיים
using System.Collections.Generic;
using System.Threading.Tasks;
using Dto;

namespace Bo.Interfaces
{
    public interface IImportService
    {
        Task ImportChildDataAsync(List<ParentChildImportDto> data);
        Task ImportKindergartenDataAsync(List<KindergartenDto> data); // זו הפונקציה החדשה/קיימת שמשתמשים בה
    }
}