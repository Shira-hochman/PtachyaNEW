using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dto;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Bo.Interfaces; // ⭐️ חובה: ייבוא הממשק הנכון!

namespace Bo.Services
{
    // ⭐️ חובה: הוספת ממשק היישום (interface implementation)
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly string _secretKey;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
            _secretKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Secret Key is not configured.");
        }

        public string GenerateToken(ChildDto child)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, child.ChildId.ToString()),
                new Claim(ClaimTypes.Role, "Parent"),
                new Claim("ChildIdNumber", child.IdNumber)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(24),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}