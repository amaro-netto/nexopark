namespace NexoPark.Core.DTOs
{
    // DTO de entrada para criar um novo veículo.
    public record VeiculoRequest(string Placa, string Modelo, string Cor);
}