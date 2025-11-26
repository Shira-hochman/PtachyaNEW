// Bo.Interfaces/IFileStorageService.cs

using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Bo.Interfaces;

public interface IFileStorageService
{
    // שומר קובץ IFormFile ומחזיר את הנתיב/URL היחסי
    Task<string> SaveFileAsync(IFormFile file, string containerName);

    // שומר מערך בתים (כגון קובץ PDF שנוצר) ומחזיר את הנתיב/URL היחסי
    Task<string> SaveBytesAsync(byte[] fileBytes, string fileName, string containerName);
}