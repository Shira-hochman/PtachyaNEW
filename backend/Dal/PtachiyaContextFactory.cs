// File: Dal/PtachiyaContextFactory.cs

// ... (שאר ה-using) ...
using Dal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Reflection;

namespace Dal
{
    public class PtachiyaContextFactory : IDesignTimeDbContextFactory<PtachiyaContext>
    {
        public PtachiyaContext CreateDbContext(string[] args)
        {
            var environmentName = "Development";

            // ⭐️⭐️⭐️ התיקון המדויק: טיפוס רמה אחת למעלה ואז כניסה ל-WebApplication1 ⭐️⭐️⭐️

            var currentDirectory = Directory.GetCurrentDirectory();

            // מטפס רמה אחת למעלה (מ-Dal ל-backend) ואז נכנס ל-WebApplication1
            var apiPath = Path.GetFullPath(Path.Combine(currentDirectory, "..", "WebApplication1"));

            // 2. בניית הקונפיגורציה
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(apiPath) // ⭐️⭐️⭐️ הנתיב הנכון לתיקיית ה-API
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: false, reloadOnChange: true)
                .Build();

            // 3. שליפת מחרוזת החיבור
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                // אם עדיין לא נמצא, זורק שגיאה עם הנתיב כדי שנוכל לדעת בדיוק איפה הבעיה
                throw new InvalidOperationException($"מחרוזת החיבור 'DefaultConnection' לא נמצאה. ודא ש-appsettings.Development.json קיים בנתיב: {apiPath}");
            }

            // 4. הגדרת האפשרויות וה-Provider
            var builder = new DbContextOptionsBuilder<PtachiyaContext>();
            builder.UseSqlServer(connectionString);

            return new PtachiyaContext(builder.Options);
        }
    }
}