using System.Security.Claims;
using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer; // Necessário para JwtBearerDefaults
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens; // Necessário para SymmetricSecurityKey, TokenValidationParameters
using Microsoft.OpenApi.Models; 
using NexoPark.Core.Constants; // <-- Crucial para AppRoles (Passo 3.4)
using NexoPark.Core.DTOs; // <-- DTOs (Passo 3.1)
using NexoPark.Core.Services; // <-- IAuthService, IJwtService (Passo 3.1)
using NexoPark.Infra.Context;
using NexoPark.Infra.Services; // <-- AuthService, JwtService (Passo 3.1)

// 1. Carrega as variáveis do .env (DB e JWT)
Env.Load();
var connectionString = Environment.GetEnvironmentVariable("NEXOPARK_CONNECTION_STRING");
var jwtSecret = Environment.GetEnvironmentVariable("JWT_KEY");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");

var builder = WebApplication.CreateBuilder(args);

// Adicionar JWT Configurações lidas do Environment
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["Jwt:Key"] = jwtSecret,
    ["Jwt:Issuer"] = jwtIssuer
});

// Adiciona o DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, b => b.MigrationsAssembly("NexoPark.Infra"))
);

// Configuração do JWT: Adiciona autenticação
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Mudar para true em produção (HTTPS/TLS)
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, 
        ValidateAudience = false, 
        ValidateLifetime = true, 
        ValidateIssuerSigningKey = true, 
        ValidIssuer = jwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret 
            ?? throw new InvalidOperationException("JWT Secret not configured.")))
    };
});

// Adicionar Serviço de Autorização
builder.Services.AddAuthorization();


// Registrar Serviços para Injeção de Dependência (DIP)
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>(); 


// Add Swagger/OpenAPI support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Adiciona o esquema de segurança JWT Bearer
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Insira o token JWT no formato 'Bearer {token}'",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Define o requisito de segurança global
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = OpenApiReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Adicionar Middlewares de Autenticação e Autorização (ORDEM IMPORTANTE!)
app.UseAuthentication();
app.UseAuthorization();

// 1. Rota de Login (Pública)
app.MapPost("/login", async (LoginRequest request, IAuthService authService) =>
{
    var response = await authService.LoginAsync(request);
    return response == null ? Results.Unauthorized() : Results.Ok(response);
})
.WithOpenApi();

// 2. Rota Protegida (Exemplo Básico)
app.MapGet("/me", (HttpContext http) =>
{
    // Captura as claims (informações) do usuário logado
    var email = http.User.FindFirst(ClaimTypes.Name)?.Value;
    var role = http.User.FindFirst(ClaimTypes.Role)?.Value;
    return Results.Ok(new { message = $"Autenticado como: {email}", role = role });
})
.RequireAuthorization() // Exige autenticação
.WithOpenApi();

// Rota Home
app.MapGet("/", () => "NexoPark API está rodando!").WithOpenApi();

app.Run();