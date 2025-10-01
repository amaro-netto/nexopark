using Microsoft.EntityFrameworkCore;
using NexoPark.Core.Entities;

namespace NexoPark.Infra.Context
{
    // O DbContext herda de DbContext e representa nossa sessão com o banco de dados.
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Mapeamento para tabelas no banco de dados:
        public DbSet<Administrador> Administradores { get; set; } = default!;
        public DbSet<Veiculo> Veiculos { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuração específica para a tabela Administrador
            modelBuilder.Entity<Administrador>(entity =>
            {
                // Configurações de chave primária e índices.
                entity.HasKey(e => e.Id);
                
                // Garante que o Email seja único e não nulo, crucial para o login.
                entity.HasIndex(e => e.Email).IsUnique(); 
                
                // Mapeamento explícito para a coluna SenhaHash, para maior clareza.
                entity.Property(e => e.SenhaHash).IsRequired();
                
                // Exemplo de índice para otimização de busca
                entity.HasIndex(e => e.Role);
            });

            // Configuração específica para a tabela Veiculo
            modelBuilder.Entity<Veiculo>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Garante que a Placa seja única (regra de negócio de um estacionamento).
                entity.HasIndex(e => e.Placa).IsUnique();
                
                // Relacionamento um-para-muitos: Um Administrador pode registrar N Veículos.
                entity.HasOne(v => v.Administrador)
                      .WithMany()
                      .HasForeignKey(v => v.AdministradorId)
                      .IsRequired(false) // Permite que o campo AdministradorId seja nulo.
                      .OnDelete(DeleteBehavior.Restrict); // Evita a exclusão acidental do Administrador.
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}