using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SistemaDeGestaoComercial.Aplicacao.Abstractions;
using SistemaDeGestaoComercial.Aplicacao.Contratos;
using SistemaDeGestaoComercial.Dominio.Entidades;
using SistemaDeGestaoComercial.Infraestrutura;
using SistemaDeGestaoComercial.Infraestrutura.Persistencia;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder
    .Services.AddControllers()
    .AddJsonOptions(opcoesJson => opcoesJson.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddCors(opcoesCors =>
    opcoesCors.AddPolicy(
        "frontend",
        politicaCors =>
            politicaCors
                .WithOrigins(builder.Configuration["Frontend:Url"] ?? "http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
    )
);
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks().AddCheck<SqlServerHealthCheck>("sqlserver");
builder.Services.AddRateLimiter(opcoes =>
{
    opcoes.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    opcoes.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(contexto =>
        RateLimitPartition.GetFixedWindowLimiter(
            contexto.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 300,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true,
            }
        )
    );
    opcoes.AddPolicy(
        "login",
        contexto =>
            RateLimitPartition.GetFixedWindowLimiter(
                contexto.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true,
                }
            )
    );
});
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opcoesSwagger =>
{
    opcoesSwagger.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Informe apenas o token JWT. O navegador da aplicação usa cookie HttpOnly.",
        }
    );
    opcoesSwagger.AddSecurityRequirement(documento => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", documento)] = [],
    });
});
var jwtKey =
    builder.Configuration["Jwt:Key"]
    ?? Environment.GetEnvironmentVariable("GESTAO_JWT_KEY")
    ?? throw new InvalidOperationException("Configure GESTAO_JWT_KEY com ao menos 32 bytes.");
if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException("GESTAO_JWT_KEY deve possuir pelo menos 32 bytes.");
builder.Configuration["Jwt:Key"] = jwtKey;
builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opcoesJwt =>
    {
        opcoesJwt.MapInboundClaims = false;
        opcoesJwt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(1),
        };
        opcoesJwt.Events = new JwtBearerEvents
        {
            OnMessageReceived = contexto =>
            {
                if (string.IsNullOrWhiteSpace(contexto.Token))
                    contexto.Token = contexto.Request.Cookies["gestao_access_token"];
                return Task.CompletedTask;
            },
            OnTokenValidated = async contexto =>
            {
                var usuarioIdValido = Guid.TryParse(
                    contexto.Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub),
                    out var usuarioId
                );
                var versaoValida = int.TryParse(
                    contexto.Principal?.FindFirstValue("token_version"),
                    out var versaoToken
                );
                var perfilValido = Enum.TryParse<PerfilUsuario>(
                    contexto.Principal?.FindFirstValue(ClaimTypes.Role),
                    out var perfil
                );
                if (!usuarioIdValido || !versaoValida || !perfilValido)
                {
                    contexto.Fail("Token inválido.");
                    return;
                }
                var validador = contexto.HttpContext.RequestServices.GetRequiredService<IValidacaoSessaoService>();
                if (
                    !await validador.UsuarioPodeAcessarAsync(
                        usuarioId,
                        versaoToken,
                        perfil,
                        contexto.HttpContext.RequestAborted
                    )
                )
                    contexto.Fail("Sessão revogada.");
            },
        };
    });
builder.Services.AddAuthorization();
builder.Services.Configure<ForwardedHeadersOptions>(opcoes =>
{
    opcoes.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});
builder.Services.AddInfraestrutura(builder.Configuration);

var app = builder.Build();
app.Use(
    async (contexto, proximo) =>
    {
        var correlationId = contexto.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? contexto.TraceIdentifier;
        contexto.TraceIdentifier = correlationId;
        contexto.Response.Headers["X-Correlation-ID"] = correlationId;
        using (app.Logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
            await proximo();
    }
);
if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    await DadosIniciais.InicializarAsync(
        scope.ServiceProvider.GetRequiredService<AppDbContext>(),
        scope.ServiceProvider.GetRequiredService<ISenhaService>(),
        app.Configuration,
        CancellationToken.None
    );
}
app.UseExceptionHandler(handler =>
    handler.Run(async context =>
    {
        var erro = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var status =
            erro is CredenciaisInvalidasException ? StatusCodes.Status401Unauthorized
            : erro is EntidadeNaoEncontradaException ? StatusCodes.Status404NotFound
            : erro is ConflitoPersistenciaException ? StatusCodes.Status409Conflict
            : erro is ExcecaoDominio or ArgumentException ? StatusCodes.Status400BadRequest
            : StatusCodes.Status500InternalServerError;
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = status,
                Title = status switch
                {
                    400 => "Regra de negócio inválida",
                    401 => "Credenciais inválidas",
                    404 => "Recurso não encontrado",
                    409 => "Conflito ao salvar os dados",
                    _ => "Erro interno",
                },
                Detail = status is 400 or 401 or 404 or 409 ? erro?.Message : "Ocorreu um erro inesperado.",
                Instance = context.Request.Path,
            }
        );
    })
);
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseForwardedHeaders();
app.UseHttpsRedirection();
if (!app.Environment.IsDevelopment())
    app.UseHsts();
app.Use(
    async (contexto, proximo) =>
    {
        contexto.Response.Headers.ContentSecurityPolicy =
            "default-src 'self'; frame-ancestors 'none'; object-src 'none'; base-uri 'self'";
        contexto.Response.Headers.XContentTypeOptions = "nosniff";
        contexto.Response.Headers["Referrer-Policy"] = "no-referrer";
        await proximo();
    }
);
app.UseCors("frontend");
app.Use(
    async (contexto, proximo) =>
    {
        if (
            contexto.Request.Method is not ("GET" or "HEAD" or "OPTIONS")
            && contexto.Request.Headers.Origin.Count > 0
            && !string.Equals(
                contexto.Request.Headers.Origin.ToString(),
                app.Configuration["Frontend:Url"] ?? "http://localhost:5173",
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            contexto.Response.StatusCode = StatusCodes.Status403Forbidden;
            await contexto.Response.WriteAsJsonAsync(
                new ProblemDetails { Status = 403, Title = "Origem da requisição não autorizada." }
            );
            return;
        }
        await proximo();
    }
);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", utc = DateTime.UtcNow })).AllowAnonymous();
app.MapHealthChecks("/health/ready", new HealthCheckOptions()).AllowAnonymous();
app.Run();

public partial class Program;

internal sealed class SqlServerHealthCheck(AppDbContext contexto) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    ) =>
        await contexto.Database.CanConnectAsync(cancellationToken)
            ? HealthCheckResult.Healthy("SQL Server acessível.")
            : HealthCheckResult.Unhealthy("SQL Server indisponível.");
}
