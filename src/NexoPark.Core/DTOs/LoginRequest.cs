namespace NexoPark.Core.DTOs
{
    // DTO de entrada para o endpoint /login
    public record LoginRequest(string Email, string Senha);
}