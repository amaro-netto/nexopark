using Microsoft.EntityFrameworkCore;
using NexoPark.Core.Entities;
using NexoPark.Core.Services;
using NexoPark.Infra.Context;

namespace NexoPark.Infra.Services
{
    // Implementação da lógica de acesso a dados (persiste no DB).
    public class VeiculoRepository : IVeiculoRepository
    {
        private readonly ApplicationDbContext _context;

        public VeiculoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Veiculo veiculo)
        {
            await _context.Veiculos.AddAsync(veiculo);
            await _context.SaveChangesAsync();
        }

        public async Task<Veiculo?> GetByPlacaAsync(string placa)
        {
            return await _context.Veiculos
                .FirstOrDefaultAsync(v => v.Placa == placa);
        }
    }
}