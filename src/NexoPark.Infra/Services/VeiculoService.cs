using Microsoft.EntityFrameworkCore;
using NexoPark.Core.DTOs;
using NexoPark.Core.Entities;
using NexoPark.Core.Services;
using NexoPark.Infra.Context;

namespace NexoPark.Infra.Services
{
    // Lógica de negócio (validação, criação do objeto).
    public class VeiculoService : IVeiculoService
    {
        private readonly IVeiculoRepository _veiculoRepository;
        private readonly ApplicationDbContext _context;

        public VeiculoService(IVeiculoRepository veiculoRepository, ApplicationDbContext context)
        {
            _veiculoRepository = veiculoRepository;
            _context = context;
        }

        public async Task<Veiculo> CriarVeiculoAsync(VeiculoRequest request, string administradorEmail)
        {
            // 1. Validação de Regra de Negócio: Placa deve ser única
            var existingVehicle = await _veiculoRepository.GetByPlacaAsync(request.Placa);
            if (existingVehicle != null)
            {
                throw new InvalidOperationException($"O veículo com a placa {request.Placa} já está registrado.");
            }

            // 2. Encontrar o Administrador logado para fins de auditoria
            var admin = await _context.Administradores
                .FirstOrDefaultAsync(a => a.Email == administradorEmail);
            
            if (admin == null)
            {
                // Isso não deve acontecer se o JWT for válido, mas é uma verificação de segurança.
                throw new UnauthorizedAccessException("Administrador logado não encontrado."); 
            }

            // 3. Criação da Entidade
            var novoVeiculo = new Veiculo(
                request.Placa.ToUpper(), 
                request.Modelo, 
                request.Cor, 
                admin.Id
            );

            // 4. Persistência
            await _veiculoRepository.AddAsync(novoVeiculo);
            
            return novoVeiculo;
        }
    }
}