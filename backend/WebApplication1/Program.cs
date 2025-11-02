using Bo.Interfaces;
using Bo.Services;
using Dal.Models; // ה-DbContext שלך
using Dal.Repositories;
using Dal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Ptachya.DAL.Repositories;
using OfficeOpenXml;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authentication.JwtBearer; // ✅ ודא שזה קיים!
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication; // ⬅️ יש לוודא שגם ה-using הזה קיים, למרות שהוא לא נשלח, ליתר ביטחון

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

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not configured");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
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
builder.Services.AddScoped<IFormRepository, FormRepository>();

builder.Services.AddScoped<IFormService, FormService>();
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

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();