using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SistemaDeGestaoComercial.Dominio.Entidades;
using SistemaDeGestaoComercial.Infraestrutura.Persistencia;

namespace SistemaDeGestaoComercial.Testes.Integracao;

public sealed class RequerSqlServerFactAttribute : FactAttribute
{
    public RequerSqlServerFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GESTAO_TEST_SQLSERVER")))
            Skip = "Defina GESTAO_TEST_SQLSERVER para executar testes reais com SQL Server.";
    }
}

public sealed class SqlServerIntegrationTests : IClassFixture<SqlServerApiFactory>
{
    private readonly SqlServerApiFactory factory;

    public SqlServerIntegrationTests(SqlServerApiFactory factory) => this.factory = factory;

    [RequerSqlServerFact]
    public async Task Migrations_EstaoIntegralmenteAplicadas()
    {
        await using var contexto = factory.CriarContexto();
        Assert.Empty(await contexto.Database.GetPendingMigrationsAsync());
    }

    [RequerSqlServerFact]
    public async Task Venda_IdempotenciaCancelamentoEEstorno_SaoAtomicos()
    {
        using var client = await factory.CriarClienteAdministradorAsync();
        var produto = await factory.CriarProdutoComEstoqueAsync(10);
        var chave = Guid.NewGuid().ToString("N");
        var entrada = new
        {
            clienteId = (Guid?)null,
            desconto = 0m,
            formaPagamento = "Pix",
            itens = new[]
            {
                new
                {
                    produtoId = produto.Id,
                    quantidade = 2,
                    desconto = 0m,
                },
            },
        };
        using var primeira = new HttpRequestMessage(HttpMethod.Post, "/api/vendas")
        {
            Content = JsonContent.Create(entrada),
        };
        primeira.Headers.Add("Idempotency-Key", chave);
        var primeiraResposta = await client.SendAsync(primeira);
        var venda = await primeiraResposta.Content.ReadFromJsonAsync<VendaResposta>();
        Assert.Equal(HttpStatusCode.Created, primeiraResposta.StatusCode);
        Assert.NotNull(venda);

        using var repetida = new HttpRequestMessage(HttpMethod.Post, "/api/vendas")
        {
            Content = JsonContent.Create(entrada),
        };
        repetida.Headers.Add("Idempotency-Key", chave);
        var repetidaResposta = await client.SendAsync(repetida);
        var vendaRepetida = await repetidaResposta.Content.ReadFromJsonAsync<VendaResposta>();
        Assert.Equal(venda!.Id, vendaRepetida!.Id);

        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync($"/api/vendas/{venda.Id}/cancelar", null)).StatusCode);
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (await client.PostAsync($"/api/vendas/{venda.Id}/cancelar", null)).StatusCode
        );

        await using var contexto = factory.CriarContexto();
        Assert.Equal(1, await contexto.Vendas.CountAsync(item => item.Id == venda.Id));
        Assert.Equal(
            1,
            await contexto.MovimentacoesFinanceiras.CountAsync(item =>
                item.VendaId == venda.Id && item.TipoMovimentacao == TipoMovimentacaoFinanceira.Estorno
            )
        );
        Assert.Equal(10, (await contexto.Produtos.SingleAsync(item => item.Id == produto.Id)).QuantidadeEstoque);
    }

    [RequerSqlServerFact]
    public async Task VendaSemEstoque_FazRollbackCompleto()
    {
        using var client = await factory.CriarClienteAdministradorAsync();
        var produto = await factory.CriarProdutoComEstoqueAsync(0);
        var vendasAntes = await factory.ContarVendasAsync();
        var chave = Guid.NewGuid().ToString("N");
        using var requisicao = new HttpRequestMessage(HttpMethod.Post, "/api/vendas")
        {
            Content = JsonContent.Create(
                new
                {
                    clienteId = (Guid?)null,
                    desconto = 0m,
                    formaPagamento = "Pix",
                    itens = new[]
                    {
                        new
                        {
                            produtoId = produto.Id,
                            quantidade = 1,
                            desconto = 0m,
                        },
                    },
                }
            ),
        };
        requisicao.Headers.Add("Idempotency-Key", chave);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.SendAsync(requisicao)).StatusCode);

        await using var contexto = factory.CriarContexto();
        Assert.Equal(vendasAntes, await contexto.Vendas.CountAsync());
        Assert.False(await contexto.RegistrosIdempotencia.AnyAsync(item => item.Chave == chave));
    }

    [RequerSqlServerFact]
    public async Task Produto_RowVersionDetectaAtualizacaoConcorrente()
    {
        var produto = await factory.CriarProdutoComEstoqueAsync(5);
        await using var primeiroContexto = factory.CriarContexto();
        await using var segundoContexto = factory.CriarContexto();
        var primeiroProduto = await primeiroContexto.Produtos.SingleAsync(item => item.Id == produto.Id);
        var segundoProduto = await segundoContexto.Produtos.SingleAsync(item => item.Id == produto.Id);
        primeiroProduto.Movimentar(TipoMovimentacaoEstoque.Entrada, 1, null, null, "teste");
        segundoProduto.Movimentar(TipoMovimentacaoEstoque.Entrada, 1, null, null, "teste");
        await primeiroContexto.SaveChangesAsync();
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => segundoContexto.SaveChangesAsync());
    }

    private sealed record VendaResposta(Guid Id, string Numero);
}

public sealed class SqlServerApiFactory : WebApplicationFactory<Program>
{
    private const string JwtKey = "integration-sqlserver-key-with-at-least-32-bytes";
    private readonly string connectionString;

    public SqlServerApiFactory()
    {
        var baseConnection = Environment.GetEnvironmentVariable("GESTAO_TEST_SQLSERVER");
        if (string.IsNullOrWhiteSpace(baseConnection))
        {
            connectionString = string.Empty;
            return;
        }
        var builder = new SqlConnectionStringBuilder(baseConnection)
        {
            InitialCatalog = $"GestaoComercialTests_{Guid.NewGuid():N}",
            Encrypt = false,
            TrustServerCertificate = true,
        };
        connectionString = builder.ConnectionString;
        using var contexto = CriarContexto();
        contexto.Database.Migrate();
        contexto.Usuarios.Add(
            new Usuario(
                "Administrador",
                "admin@teste.local",
                CriarHash("Senha123"),
                PerfilUsuario.Administrador,
                "teste"
            )
        );
        contexto.SaveChanges();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        if (string.IsNullOrEmpty(connectionString))
            return;
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration(
            (_, configuracao) =>
                configuracao.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:SqlServer"] = connectionString,
                        ["Jwt:Key"] = JwtKey,
                        ["Negocio:FusoHorario"] = "America/Sao_Paulo",
                    }
                )
        );
    }

    public AppDbContext CriarContexto() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(connectionString).Options);

    public async Task<HttpClient> CriarClienteAdministradorAsync()
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        var resposta = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = "admin@teste.local", senha = "Senha123" }
        );
        resposta.EnsureSuccessStatusCode();
        return client;
    }

    public async Task<Produto> CriarProdutoComEstoqueAsync(int quantidade)
    {
        await using var contexto = CriarContexto();
        var produto = new Produto(Guid.NewGuid().ToString("N"), "Produto teste", null, 5m, 10m, 1, "teste");
        contexto.Produtos.Add(produto);
        if (quantidade > 0)
            contexto.MovimentacoesEstoque.Add(
                produto.Movimentar(TipoMovimentacaoEstoque.Entrada, quantidade, null, "Carga de teste", "teste")
            );
        await contexto.SaveChangesAsync();
        return produto;
    }

    public async Task<int> ContarVendasAsync()
    {
        await using var contexto = CriarContexto();
        return await contexto.Vendas.CountAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !string.IsNullOrEmpty(connectionString))
        {
            using var contexto = CriarContexto();
            contexto.Database.EnsureDeleted();
        }
        base.Dispose(disposing);
    }

    private static string CriarHash(string senha)
    {
        const int iteracoes = 210_000;
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(senha, salt, iteracoes, HashAlgorithmName.SHA512, 32);
        return $"{iteracoes}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }
}
