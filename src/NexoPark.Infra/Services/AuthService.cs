using BCrypt.Net;
using Microsoft.EntityFrameworkCore; // <-- Corrigido o erro CS0234 EFCore
using NexoPark.Core.DTOs;
using NexoPark.Core.Services;
using NexoPark.Infra.Context; // <-- Corrigido o erro CS0234 Context

namespace NexoPark.Infra.Services // <-- O namespace deve ser NexoPark.Infra.Services
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
            var admin = await _context.Administradores
                .FirstOrDefaultAsync(a => a.Email == request.Email);

            if (admin == null) return null; 

            if (!BCrypt.Net.BCrypt.Verify(request.Senha, admin.SenhaHash))
            {
                return null;
            }

            var token = _jwtService.GenerateToken(admin.Email, admin.Role);

            return new LoginResponse(token);
        }
    }
}