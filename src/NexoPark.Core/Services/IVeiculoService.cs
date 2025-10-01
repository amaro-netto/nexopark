using NexoPark.Core.DTOs;
using NexoPark.Core.Entities;

namespace NexoPark.Core.Services
{
    // Contrato para a lógica de negócio dos veículos.
    public interface IVeiculoService
    {
        // Cria um veículo e o associa ao administrador que está logado (identificado pelo seu email).
        Task<Veiculo> CriarVeiculoAsync(VeiculoRequest request, string administradorEmail);
    }
}