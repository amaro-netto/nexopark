using System.Security.Claims;
using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NexoPark.Core.Constants;
using NexoPark.Core.DTOs;
using NexoPark.Core.Services;
using NexoPark.Infra.Context;
using NexoPark.Infra.Services;

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

// Adicionar Serviço de Autorização e Políticas Baseadas em Roles
builder.Services.AddAuthorization(options =>
{
    // Política para usuários com perfil Admin (acesso total)
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole(AppRoles.Admin));
    
    // Política para usuários com perfil Admin OU Editor (acesso básico/escrita)
    options.AddPolicy("RequireEditorOrAdmin", policy => 
        policy.RequireRole(AppRoles.Editor, AppRoles.Admin));
});


// Registrar Serviços para Injeção de Dependência (DIP)
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>(); 
// Adições para Veículo
builder.Services.AddScoped<IVeiculoRepository, VeiculoRepository>(); // Repository Pattern
builder.Services.AddScoped<IVeiculoService, VeiculoService>(); // Service de Negócio



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

    // Definir a política de 
    /*
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy",
        builder => builder
            .WithOrigins("http://localhost:3000") // Permite o Frontend Next.js
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()); // Necessário para futura autenticação com HttpOnly cookies
    });
    */
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// USAR A POLÍTICA DE CORS AQUI (DEVE VIR ANTES DE UseAuthentication/UseAuthorization)
/*app.UseCors("CorsPolicy"); 
*/
// Adicionar Middlewares de Autenticação e Autorização (ORDEM IMPORTANTE!)
app.UseAuthentication();
app.UseAuthorization();


// 1. Rota de Login (Pública)
app.MapPost("/login", async (LoginRequest request, IAuthService authService) =>
{
    var response = await authService.LoginAsync(request);
    
    // Agora o servidor retornará o token real do AuthService (que é uma string longa).
    return response == null ? Results.Unauthorized() : Results.Ok(response);
})
.AllowAnonymous(); // MANTÉM .AllowAnonymous()


// 2. Rota Protegida com Autorização (TESTE 1: Acesso somente para Admin)
app.MapGet("/admin-only", (HttpContext http) =>
{
    var email = http.User.FindFirst(ClaimTypes.Name)?.Value;
    return Results.Ok($"Bem-vindo, Admin {email}! Acesso exclusivo.");
})
.RequireAuthorization("RequireAdminRole");

// 3. Rota Protegida com Autorização (TESTE 2: Acesso para Admin ou Editor)
app.MapGet("/editor-or-admin", (HttpContext http) =>
{
    var email = http.User.FindFirst(ClaimTypes.Name)?.Value;
    return Results.Ok($"Bem-vindo, {http.User.FindFirst(ClaimTypes.Role)?.Value} {email}! Acesso de Leitura/Escrita.");
})
.RequireAuthorization("RequireEditorOrAdmin");

// 4. Rota POST para criar Veículo (Protegida)
app.MapPost("/veiculos", async (VeiculoRequest request, IVeiculoService veiculoService, HttpContext http) =>
{
    // Captura o email do token (ClaimTypes.Name)
    var administradorEmail = http.User.FindFirst(ClaimTypes.Name)?.Value;

    if (string.IsNullOrEmpty(administradorEmail))
    {
        return Results.Unauthorized();
    }

    try
    {
        var veiculo = await veiculoService.CriarVeiculoAsync(request, administradorEmail);
        // Resposta 201 Created com o corpo do novo recurso
        return Results.Created($"/veiculos/{veiculo.Id}", veiculo);
    }
    catch (InvalidOperationException ex)
    {
        // Erro de regra de negócio (ex: Placa já existe)
        return Results.Conflict(new { error = ex.Message });
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Forbid();
    }
    catch (Exception)
    {
        return Results.Problem("Ocorreu um erro interno ao registrar o veículo.");
    }
});

// 5. Rota GET para retornar todos os Veículos (Protegida)
app.MapGet("/veiculos", async (IVeiculoService veiculoService) =>
{
    var veiculos = await veiculoService.ListarVeiculosAsync();
    
    // Retorna 200 OK. Se a lista estiver vazia, retorna uma lista vazia ([])
    return Results.Ok(veiculos);
})

// Requer Admin OU Editor para criar veículos
.RequireAuthorization("RequireEditorOrAdmin");
// .WithOpenApi(); // MANTEMOS COMENTADO

// Rota Home
app.MapGet("/", () => "NexoPark API está rodando!");

app.Run();