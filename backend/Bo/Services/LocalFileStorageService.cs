using Bo.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _baseDirectory;
    private readonly string _baseUrl;

    public LocalFileStorageService(IConfiguration configuration)
    {
        _baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
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

        // מחזירים רק נתיב לוגי, לא פיזי
        return $"{containerName}/{uniqueFileName}";
    }

    public async Task<string> SaveBytesAsync(byte[] fileBytes, string fileName, string containerName)
    {
        var directory = Path.Combine(_baseDirectory, containerName);
        Directory.CreateDirectory(directory);

        string filePath = Path.Combine(directory, fileName);
        await File.WriteAllBytesAsync(filePath, fileBytes);

        return $"{containerName}/{fileName}";
    }
}
