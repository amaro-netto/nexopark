using System.IdentityModel.Tokens.Jwt; // <-- Corrigido erro System.IdentityModel
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration; // <-- Corrigido erro Microsoft.Extensions
using Microsoft.IdentityModel.Tokens;
using NexoPark.Core.Services;

namespace NexoPark.Infra.Services // <-- O namespace deve ser NexoPark.Infra.Services
{
    public class JwtService : IJwtService
    {
        private readonly string _jwtSecret;
        private readonly string _jwtIssuer;
        
        public JwtService(IConfiguration configuration)
        {
            // ... restante do código sem alterações ...
            _jwtSecret = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Secret not configured.");
            _jwtIssuer = configuration["Jwt:Issuer"] ?? "NexoParkAPI";
        }

        // ... restante do método GenerateToken ...
        public string GenerateToken(string email, string role)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret); 
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, email),
                    new Claim(ClaimTypes.Role, role)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = _jwtIssuer,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}