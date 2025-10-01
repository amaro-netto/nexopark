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

    // Bloco COMENTADO para resolver o erro CS0103.
    // A funcionalidade de segurança no Swagger UI não estará visível, mas a API funciona.
    /*
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

// Adicionar Middlewares de Autenticação e Autorização (ORDEM IMPORTANTE!)
app.UseAuthentication();
app.UseAuthorization();

// 1. Rota de Login (Pública)
app.MapPost("/login", async (LoginRequest request, IAuthService authService) =>
{
    var response = await authService.LoginAsync(request);
    return response == null ? Results.Unauthorized() : Results.Ok(response);
}); // Finaliza com ; (Ponto e vírgula)

// 2. Rota Protegida com Autorização (TESTE 1: Acesso somente para Admin)
app.MapGet("/admin-only", (HttpContext http) =>
{
    var email = http.User.FindFirst(ClaimTypes.Name)?.Value;
    return Results.Ok($"Bem-vindo, Admin {email}! Acesso exclusivo.");
})
.RequireAuthorization("RequireAdminRole"); // Finaliza com ;

// 3. Rota Protegida com Autorização (TESTE 2: Acesso para Admin ou Editor)
app.MapGet("/editor-or-admin", (HttpContext http) =>
{
    var email = http.User.FindFirst(ClaimTypes.Name)?.Value;
    return Results.Ok($"Bem-vindo, {http.User.FindFirst(ClaimTypes.Role)?.Value} {email}! Acesso de Leitura/Escrita.");
})
.RequireAuthorization("RequireEditorOrAdmin"); // Finaliza com ;

// Rota Home
app.MapGet("/", () => "NexoPark API está rodando!"); // Finaliza com ;

app.Run();