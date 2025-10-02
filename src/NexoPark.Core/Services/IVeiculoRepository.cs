using NexoPark.Core.Entities;

namespace NexoPark.Core.Services
{
    // Contrato para acesso a dados de veículos.
    public interface IVeiculoRepository
    {
        // Operações CRUD básicas (futuras)
        Task AddAsync(Veiculo veiculo);
        Task<Veiculo?> GetByPlacaAsync(string placa); // Útil para validação

        // NOVO: Retorna todos os veículos
        Task<List<Veiculo>> GetAllAsync(); 
        // ... outros (GetById, Update, Delete)
    }
}