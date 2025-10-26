using Bo.Interfaces;
using Bo.Services;
using Dal.Models; // ה-DbContext שלך
using Dal.Repositories;
using Dal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Ptachya.DAL.Repositories;
using OfficeOpenXml; // ודא שאתה משתמש ב-using זה
ExcelPackage.License.SetNonCommercialPersonal("שם פרטי");
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowSpecificOrigin", // נותנים שם למדיניות
        policy =>
        {
            policy.WithOrigins("http://localhost:4200") // ⬅️ המקור שצריך אישור (הכתובת של Angular)
                  .AllowAnyHeader()                  // מאפשר כל כותרת
                  .AllowAnyMethod();                 // מאפשר כל מתודה (GET, POST, OPTIONS וכו')
        });
});

// --- הגדרות Middleware (אחרי app.Build()) ---

// 4. שימוש ב-CORS באמצעות המשתנה app
// ⬅️ התיקון: הגדרה נכונה של קונטקסט הרישיון לגרסאות EPPlus 8 ומעלה

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddDbContext<PtachiyaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//// 🔑 הרשמה לרפוזיטוריז (DAL & BO)
builder.Services.AddScoped<IChildRepository, ChildRepository>();
builder.Services.AddScoped<IChildService, ChildService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

//// הרשמה לממשקי גני ילדים
builder.Services.AddScoped<IKindergartenRepository, KindergartenRepository>();
builder.Services.AddScoped<IKindergartenService, KindergartenService>();

//// הוספת הרשמה לממשקי ההורים
//builder.Services.AddScoped<IParentRepository, ParentRepository>();
//builder.Services.AddScoped<IParentService, ParentService>();
builder.Services.AddScoped<Bo.Interfaces.IImportService, Bo.Services.ImportService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCors("AllowFrontend"); // ⬅️ כאן משתמשים ב-app

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();