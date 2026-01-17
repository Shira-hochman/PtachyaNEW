using Bo.Interfaces;
using Bo.Services;
using Dal.Models;
using Dal.Repositories;
using Dal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Ptachya.DAL.Repositories;

// הגדרת רישיון EPPlus
ExcelPackage.License.SetNonCommercialPersonal("שם פרטי");

var builder = WebApplication.CreateBuilder(args);

// הגדרת שם המדיניות כמשתנה 
const string MyCorsPolicy = "AllowSpecificOrigin";

// 1. הוספת שירות CORS (AddCors)
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyCorsPolicy,
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// הגדרת JWT
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

// ⭐️ הרשמה לכל השירותים והרפוזיטוריז:

// DAL/Repositories
builder.Services.AddScoped<IChildRepository, ChildRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IKindergartenRepository, KindergartenRepository>();
builder.Services.AddScoped<IFormRepository, FormRepository>();

// BO/Services
builder.Services.AddScoped<IChildService, ChildService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IKindergartenService, KindergartenService>();
builder.Services.AddScoped<IFormService, FormService>();
builder.Services.AddScoped<Bo.Interfaces.IImportService, Bo.Services.ImportService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddHttpClient(); // חשוב מאוד לעבודה עם HttpClient
builder.Services.AddScoped<IPaymentService, KesherPaymentService>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();


// 🛑 הוספת ITokenService (פתרון שגיאת DI)
builder.Services.AddScoped<Bo.Interfaces.ITokenService, Bo.Services.TokenService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200") // הכתובת של ה-Angular
                  .AllowAnyHeader()                   // מאשר את כל ה-Headers (כולל Authorization)
                  .AllowAnyMethod();                  // מאשר POST, GET וכו'
        });
});

var app = builder.Build();

app.UseCors("AllowAngular");

// 2. הפעלת Middleware של CORS (UseCors)
app.UseCors(MyCorsPolicy);

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