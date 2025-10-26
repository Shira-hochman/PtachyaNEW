using Bo.Interfaces;
using Bo.Services;
using Dal.Models; // ה-DbContext שלך
using Dal.Repositories;
using Dal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Ptachya.DAL.Repositories;
using OfficeOpenXml;
using Microsoft.AspNetCore.Cors; // ודא ש-using זה קיים

// הגדרת רישיון EPPlus
ExcelPackage.License.SetNonCommercialPersonal("שם פרטי");

var builder = WebApplication.CreateBuilder(args);

// הגדרת שם המדיניות כמשתנה (מומלץ למניעת טעויות כתיב)
const string MyCorsPolicy = "AllowSpecificOrigin";

// 1. הוספת שירות CORS (AddCors)
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyCorsPolicy, // נותנים שם למדיניות: "AllowSpecificOrigin"
        policy =>
        {
            policy.WithOrigins("http://localhost:4200") // ⬅️ המקור של Angular
                  .AllowAnyHeader()                     // מאפשר כל כותרת
                  .AllowAnyMethod();                     // מאפשר כל מתודה
        });
});

// Add services to the container.
builder.Services.AddControllers();

// הגדרת DbContext
builder.Services.AddDbContext<PtachiyaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// הרשמה לרפוזיטוריז (DAL & BO)
builder.Services.AddScoped<IChildRepository, ChildRepository>();
builder.Services.AddScoped<IChildService, ChildService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IKindergartenRepository, KindergartenRepository>();
builder.Services.AddScoped<IKindergartenService, KindergartenService>();
builder.Services.AddScoped<Bo.Interfaces.IImportService, Bo.Services.ImportService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 2. הפעלת Middleware של CORS (UseCors)
// המיקום חשוב: צריך להיות אחרי app.Build() ולפני UseAuthorization
app.UseCors(MyCorsPolicy); // ⬅️ **התיקון המרכזי: שימוש בשם המדיניות הנכון**

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