using Bo.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration; // חובה לייבא
using System;
using System.IO;
using System.Threading.Tasks;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _baseDirectory;
    private readonly string _baseUrl;

    // הזרקת Configuration כדי לדעת מה הכתובת של השרת (למשל https://localhost:7222/)
    public LocalFileStorageService(IConfiguration configuration)
    {
        _baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        // קורא את ה-BaseUrl מתוך appsettings.json, או משתמש בברירת מחדל
        _baseUrl = configuration["AppSettings:BaseUrl"] ?? "https://localhost:7222/";
    }

    public async Task<string> SaveFileAsync(IFormFile file, string containerName)
    {
        var attachmentsDirectory = Path.Combine(_baseDirectory, containerName);
        Directory.CreateDirectory(attachmentsDirectory);

        string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
        string filePath = Path.Combine(attachmentsDirectory, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // ⭐️ שינוי קריטי: החזרת URL מלא להורדה במקום נתיב פיזי
        // פורמט: https://localhost:7222/api/Files/Download/{containerName}/{uniqueFileName}
        return $"{_baseUrl}api/Files/Download/{containerName}/{uniqueFileName}";
    }

    public async Task<string> SaveBytesAsync(byte[] fileBytes, string fileName, string containerName)
    {
        var directory = Path.Combine(_baseDirectory, containerName);
        Directory.CreateDirectory(directory);

        string filePath = Path.Combine(directory, fileName);
        await File.WriteAllBytesAsync(filePath, fileBytes);

        // ⭐️ שינוי קריטי: החזרת URL מלא להורדה
        return $"{_baseUrl}api/Files/Download/{containerName}/{fileName}";
    }
}