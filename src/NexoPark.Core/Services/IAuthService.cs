using NexoPark.Core.DTOs;

namespace NexoPark.Core.Services
{
    // Contrato para o serviço de autenticação
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
    }
}