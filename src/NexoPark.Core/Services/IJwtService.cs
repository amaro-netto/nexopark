namespace NexoPark.Core.Services
{
    public interface IJwtService
    {
        // Gera o token JWT com o email e o perfil (role) do administrador.
        string GenerateToken(string email, string role);
    }
}