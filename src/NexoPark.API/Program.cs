using DotNetEnv; // Pacote para ler o .env
using Microsoft.EntityFrameworkCore;
using NexoPark.Infra.Context;

// 1. Carrega as variáveis do .env (Segurança por Design)
Env.Load();
var connectionString = Environment.GetEnvironmentVariable("NEXOPARK_CONNECTION_STRING");

var builder = WebApplication.CreateBuilder(args);

// Adiciona o DbContext à coleção de serviços
// Utiliza a Connection String lida de forma segura.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, b => b.MigrationsAssembly("NexoPark.Infra"))
);

// Add services to the container.
// builder.Services.AddControllers(); // Não precisamos de Controllers tradicionais, usaremos Minimal APIs

// Add Swagger/OpenAPI support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Definição da rota Home (será melhorada em etapas futuras)
app.MapGet("/", () => "NexoPark API está rodando!");

app.Run();