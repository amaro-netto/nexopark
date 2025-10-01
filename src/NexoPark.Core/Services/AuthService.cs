using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using NexoPark.Core.DTOs;
using NexoPark.Core.Services;
using NexoPark.Infra.Context;

namespace NexoPark.Infra.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService; 

        public AuthService(ApplicationDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            // 1. Busca o Administrador pelo email
            var admin = await _context.Administradores
                .FirstOrDefaultAsync(a => a.Email == request.Email);

            if (admin == null)
            {
                // Segurança: Não revela se o email existe
                return null; 
            }

            // 2. Verifica a Senha Hashed (BCrypt.Verify é seguro)
            if (!BCrypt.Net.BCrypt.Verify(request.Senha, admin.SenhaHash))
            {
                return null;
            }

            // 3. Geração do Token JWT
            var token = _jwtService.GenerateToken(admin.Email, admin.Role);

            return new LoginResponse(token);
        }
    }
}