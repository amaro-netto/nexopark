namespace NexoPark.Core.Entities
{
    public class Administrador
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        
        // Armazenará o hash seguro (ex: bcrypt ou argon2) da senha.
        // NUNCA a senha em texto puro.
        public string SenhaHash { get; set; } = string.Empty; 

        // Role/Perfil de acesso para autorização JWT (ex: Admin, Editor)
        public string Role { get; set; } = string.Empty; 

        // Construtor vazio para EF Core
        public Administrador() { }

        // Construtor para inicialização
        public Administrador(string nome, string email, string senhaHash, string role)
        {
            Id = Guid.NewGuid();
            Nome = nome;
            Email = email;
            SenhaHash = senhaHash;
            Role = role;
        }
    }
}