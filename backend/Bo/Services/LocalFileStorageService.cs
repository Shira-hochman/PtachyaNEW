// Bo.Services/LocalFileStorageService.cs

using Bo.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

// שימו לב: יש לוודא שה-namespace נכון
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _baseDirectory;

    public LocalFileStorageService()
    {
        // קביעת תיקיית הבסיס (היכן שהשרת רץ)
        _baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
    }

    // יישום שמירת IFormFile (קבצים נלווים)
    public async Task<string> SaveFileAsync(IFormFile file, string containerName)
    {
        var attachmentsDirectory = Path.Combine(_baseDirectory, containerName);
        Directory.CreateDirectory(attachmentsDirectory);

        // יצירת שם קובץ ייחודי כדי למנוע דריסה
        string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
        string filePath = Path.Combine(attachmentsDirectory, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // מחזירים את שם הקובץ הייחודי בלבד (היחסי לתיקיית הבסיס)
        return Path.Combine(containerName, uniqueFileName);
    }

    // יישום שמירת byte array (ה-PDF שנוצר)
    public async Task<string> SaveBytesAsync(byte[] fileBytes, string fileName, string containerName)
    {
        var directory = Path.Combine(_baseDirectory, containerName);
        Directory.CreateDirectory(directory);

        string filePath = Path.Combine(directory, fileName);
        await File.WriteAllBytesAsync(filePath, fileBytes);

        // מחזירים את הנתיב היחסי
        return Path.Combine(containerName, fileName);
    }
}