namespace NexoPark.Core.Entities
{
    public class Veiculo
    {
        public Guid Id { get; set; }
        public string Placa { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Cor { get; set; } = string.Empty;
        
        // Campo futuro: Posição da vaga no estacionamento (para controle de vagas)
        public string? PosicaoVaga { get; set; } 
        
        public DateTime DataRegistro { get; set; }
        
        // Chave estrangeira para o Administrador que registrou o veículo (Opcional)
        public Guid? AdministradorId { get; set; }
        
        // Propriedade de navegação (para EF Core)
        public Administrador? Administrador { get; set; }

        public Veiculo()
        {
            DataRegistro = DateTime.UtcNow;
        }
        
        public Veiculo(string placa, string modelo, string cor, Guid administradorId) : this()
        {
            Id = Guid.NewGuid();
            Placa = placa;
            Modelo = modelo;
            Cor = cor;
            AdministradorId = administradorId;
        }
    }
}