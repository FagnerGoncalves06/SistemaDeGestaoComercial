using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using SistemaDeGestaoComercial.Aplicacao.Abstractions;
using SistemaDeGestaoComercial.Aplicacao.Contratos;
using SistemaDeGestaoComercial.Dominio.Entidades;

namespace SistemaDeGestaoComercial.Testes.Integracao;

public sealed class ApiTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory factory;

    public ApiTests(ApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Health_RetornaSucesso()
    {
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health")).StatusCode);
    }

    [Fact]
    public async Task EndpointProtegido_SemToken_RetornaNaoAutorizado()
    {
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/clientes")).StatusCode);
    }

    [Fact]
    public async Task Login_ComEntradaInvalida_RetornaProblemDetails()
    {
        using var client = factory.CreateClient();
        var resposta = await client.PostAsJsonAsync("/api/auth/login", new { email = "invalido", senha = "1" });
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("application/problem+json", resposta.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Login_ComEntradaValida_RetornaToken()
    {
        using var client = factory.CreateClient();
        var resposta = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = "operador@teste.local", senha = "Senha123" }
        );
        var login = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal("Operador", login.GetProperty("perfil").GetString());
        Assert.False(login.TryGetProperty("token", out _));
        Assert.Contains(
            resposta.Headers.GetValues("Set-Cookie"),
            valor =>
                valor.Contains("gestao_access_token=", StringComparison.Ordinal)
                && valor.Contains("httponly", StringComparison.OrdinalIgnoreCase)
        );
    }

    [Fact]
    public async Task Paginacao_EncaminhaTamanhoPaginaAoCasoDeUso()
    {
        using var client = factory.CriarClienteAutenticado(PerfilUsuario.Operador);
        var resposta = await client.GetAsync("/api/clientes?pagina=2&tamanhoPagina=77");
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal((2, 77), factory.ClienteService.UltimaPaginacao);
    }

    [Fact]
    public async Task Operador_EmEndpointAdministrativo_RetornaProibido()
    {
        using var client = factory.CriarClienteAutenticado(PerfilUsuario.Operador);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/dashboard")).StatusCode);
    }

    [Fact]
    public async Task ConflitoDePersistencia_RetornaStatus409()
    {
        factory.ClienteService.LancarConflitoAoCriar = true;
        try
        {
            using var client = factory.CriarClienteAutenticado(PerfilUsuario.Operador);
            var resposta = await client.PostAsJsonAsync(
                "/api/clientes",
                new
                {
                    nome = "Cliente Teste",
                    cpf = "52998224725",
                    email = "cliente@teste.local",
                    telefone = "11999999999",
                    dataNascimento = (string?)null,
                    endereco = new
                    {
                        cep = "01001000",
                        logradouro = "Praça da Sé",
                        numero = "1",
                        complemento = (string?)null,
                        bairro = "Sé",
                        cidade = "São Paulo",
                        uf = "SP",
                    },
                }
            );
            Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
        }
        finally
        {
            factory.ClienteService.LancarConflitoAoCriar = false;
        }
    }
}

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private const string JwtKey = "integration-test-key-with-at-least-32-bytes";
    public ClienteServiceFake ClienteService { get; } = new();

    public ApiFactory() => Environment.SetEnvironmentVariable("GESTAO_JWT_KEY", JwtKey);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IAutenticacaoService>();
            services.RemoveAll<IClienteService>();
            services.RemoveAll<IValidacaoSessaoService>();
            services.AddSingleton<IAutenticacaoService, AutenticacaoServiceFake>();
            services.AddSingleton<IClienteService>(ClienteService);
            services.AddSingleton<IValidacaoSessaoService, ValidacaoSessaoServiceFake>();
        });
    }

    public HttpClient CriarClienteAutenticado(PerfilUsuario perfil)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CriarToken(perfil));
        return client;
    }

    private static string CriarToken(PerfilUsuario perfil)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, "Usuário de teste"),
            new Claim(ClaimTypes.Email, "usuario@teste.local"),
            new Claim(ClaimTypes.Role, perfil.ToString()),
            new Claim("token_version", "1"),
        };
        var token = new JwtSecurityToken(
            "SistemaDeGestaoComercial",
            "SistemaDeGestaoComercial.Web",
            claims,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey)),
                SecurityAlgorithms.HmacSha256
            )
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

internal sealed class AutenticacaoServiceFake : IAutenticacaoService
{
    public Task<LoginDto> LoginAsync(LoginEntrada entrada, CancellationToken cancellationToken) =>
        Task.FromResult(
            new LoginDto("token-de-teste", DateTime.UtcNow.AddMinutes(5), "Operador", PerfilUsuario.Operador)
        );
}

internal sealed class ValidacaoSessaoServiceFake : IValidacaoSessaoService
{
    public Task<bool> UsuarioPodeAcessarAsync(
        Guid usuarioId,
        int versaoToken,
        PerfilUsuario perfil,
        CancellationToken cancellationToken
    ) => Task.FromResult(true);
}

public sealed class ClienteServiceFake : IClienteService
{
    public (int Pagina, int TamanhoPagina) UltimaPaginacao { get; private set; }
    public bool LancarConflitoAoCriar { get; set; }

    public Task<Pagina<ClienteDto>> ListarClientesAsync(
        string? busca,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken
    )
    {
        UltimaPaginacao = (pagina, tamanhoPagina);
        return Task.FromResult(new Pagina<ClienteDto>([], pagina, tamanhoPagina, 0));
    }

    public Task<ClienteDto> CriarClienteAsync(
        ClienteEntrada entrada,
        string usuarioResponsavel,
        CancellationToken cancellationToken
    ) =>
        LancarConflitoAoCriar
            ? Task.FromException<ClienteDto>(new ConflitoPersistenciaException("CPF já cadastrado."))
            : Task.FromResult(CriarCliente());

    public Task<ClienteDto> ObterClienteAsync(Guid clienteId, CancellationToken cancellationToken) =>
        Task.FromResult(CriarCliente(clienteId));

    public Task<ClienteDto> AtualizarClienteAsync(
        Guid clienteId,
        ClienteAtualizacao entrada,
        string usuarioResponsavel,
        CancellationToken cancellationToken
    ) => Task.FromResult(CriarCliente(clienteId));

    public Task ExcluirClienteAsync(Guid clienteId, string usuarioResponsavel, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task<Pagina<VendaDto>> HistoricoClienteAsync(
        Guid clienteId,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken
    ) => Task.FromResult(new Pagina<VendaDto>([], pagina, tamanhoPagina, 0));

    private static ClienteDto CriarCliente(Guid? clienteId = null) =>
        new(
            clienteId ?? Guid.NewGuid(),
            "Cliente Teste",
            "52998224725",
            "cliente@teste.local",
            "11999999999",
            null,
            new EnderecoDto("01001000", "Praça da Sé", "1", null, "Sé", "São Paulo", "SP"),
            true
        );
}
