using NexoPark.Core.Entities;

namespace NexoPark.Core.Services
{
    // Contrato para acesso a dados de veículos.
    public interface IVeiculoRepository
    {
        // Operações CRUD básicas (futuras)
        Task AddAsync(Veiculo veiculo);
        Task<Veiculo?> GetByPlacaAsync(string placa); // Útil para validação
        // ... outros (GetById, Update, Delete)
    }
}