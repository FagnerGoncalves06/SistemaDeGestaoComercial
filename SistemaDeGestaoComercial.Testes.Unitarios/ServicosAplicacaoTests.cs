using SistemaDeGestaoComercial.Aplicacao.Abstractions;
using SistemaDeGestaoComercial.Aplicacao.Servicos;
using SistemaDeGestaoComercial.Dominio.Entidades;

namespace SistemaDeGestaoComercial.Testes.Unitarios;

public sealed class ServicosAplicacaoTests
{
    [Fact]
    public async Task ValidacaoSessao_ReutilizaResultadoDoCache()
    {
        var usuario = new Usuario("Teste", "teste@local.dev", "hash-valido", PerfilUsuario.Administrador, "teste");
        var repositorio = new UsuarioRepositorioFake(usuario);
        var cache = new CacheSessaoFake();
        var servico = new ValidacaoSessaoService(repositorio, cache);

        Assert.True(
            await servico.UsuarioPodeAcessarAsync(
                usuario.Id,
                usuario.VersaoToken,
                usuario.Perfil,
                CancellationToken.None
            )
        );
        Assert.True(
            await servico.UsuarioPodeAcessarAsync(
                usuario.Id,
                usuario.VersaoToken,
                usuario.Perfil,
                CancellationToken.None
            )
        );
        Assert.Equal(1, repositorio.ConsultasPorId);
    }

    [Fact]
    public async Task Dashboard_UsaLimitesDoFusoECalculaValoresLiquidos()
    {
        var inicioDia = new DateTime(2026, 8, 27, 3, 0, 0, DateTimeKind.Utc);
        var inicioMes = new DateTime(2026, 8, 1, 3, 0, 0, DateTimeKind.Utc);
        var financeiro = new FinanceiroRepositorioFake(
            new TotaisFinanceiros(100m, 1_000m, 20m, 200m, 10m, 100m, 3, 30)
        );
        var servico = new DashboardService(
            financeiro,
            new ProdutoRepositorioFake(),
            new RelogioFake(inicioDia, inicioMes)
        );

        var dashboard = await servico.ObterAsync(CancellationToken.None);

        Assert.Equal((inicioDia, inicioMes), financeiro.LimitesRecebidos);
        Assert.Equal(90m, dashboard.FaturamentoDia);
        Assert.Equal(70m, dashboard.SaldoDia);
        Assert.Equal(30m, dashboard.TicketMedioDia);
    }
}

internal sealed class UsuarioRepositorioFake(Usuario usuario) : IUsuarioRepositorio
{
    public int ConsultasPorId { get; private set; }

    public Task<Usuario?> ObterPorIdAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        ConsultasPorId++;
        return Task.FromResult<Usuario?>(usuario.Id == usuarioId ? usuario : null);
    }

    public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    public Task<bool> ExisteEmailAsync(string email, Guid? ignorarUsuarioId, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    public Task<ResultadoPaginado<Usuario>> ListarAsync(
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken
    ) => throw new NotSupportedException();

    public void Adicionar(Usuario novoUsuario) => throw new NotSupportedException();
}

internal sealed class CacheSessaoFake : ICacheSessao
{
    private readonly Dictionary<string, bool> valores = [];

    public Task<bool?> ObterAsync(
        Guid usuarioId,
        int versaoToken,
        PerfilUsuario perfil,
        CancellationToken cancellationToken
    ) =>
        Task.FromResult(
            valores.TryGetValue(Chave(usuarioId, versaoToken, perfil), out var valor) ? (bool?)valor : null
        );

    public Task ArmazenarAsync(
        Guid usuarioId,
        int versaoToken,
        PerfilUsuario perfil,
        bool permitido,
        CancellationToken cancellationToken
    )
    {
        valores[Chave(usuarioId, versaoToken, perfil)] = permitido;
        return Task.CompletedTask;
    }

    public Task InvalidarAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        valores.Clear();
        return Task.CompletedTask;
    }

    private static string Chave(Guid usuarioId, int versaoToken, PerfilUsuario perfil) =>
        $"{usuarioId}:{versaoToken}:{perfil}";
}

internal sealed class RelogioFake(DateTime inicioDia, DateTime inicioMes) : IRelogioNegocio
{
    public DateTime UtcAgora => inicioDia;

    public (DateTime InicioDiaUtc, DateTime InicioMesUtc) ObterLimitesUtc() => (inicioDia, inicioMes);
}

internal sealed class FinanceiroRepositorioFake(TotaisFinanceiros totais) : IFinanceiroRepositorio
{
    public (DateTime InicioDia, DateTime InicioMes) LimitesRecebidos { get; private set; }

    public Task<TotaisFinanceiros> ObterTotaisAsync(
        DateTime inicioDia,
        DateTime inicioMes,
        CancellationToken cancellationToken
    )
    {
        LimitesRecebidos = (inicioDia, inicioMes);
        return Task.FromResult(totais);
    }

    public Task<ResultadoPaginado<MovimentacaoFinanceira>> ListarAsync(
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken
    ) => throw new NotSupportedException();

    public void Adicionar(MovimentacaoFinanceira movimentacao) => throw new NotSupportedException();
}

internal sealed class ProdutoRepositorioFake : IProdutoRepositorio
{
    public Task<ResultadoPaginado<Produto>> ListarAsync(
        string? busca,
        bool estoqueBaixo,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken
    ) => Task.FromResult(new ResultadoPaginado<Produto>([], pagina, tamanhoPagina, 0));

    public Task<Produto?> ObterAsync(Guid produtoId, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    public Task<Dictionary<Guid, Produto>> ObterAtivosAsync(
        IReadOnlyCollection<Guid> produtoIds,
        CancellationToken cancellationToken
    ) => throw new NotSupportedException();

    public Task<bool> ExisteCodigoAsync(string codigo, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    public Task<bool> PossuiItensVendaAsync(Guid produtoId, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    public void Adicionar(Produto produto) => throw new NotSupportedException();

    public void Remover(Produto produto) => throw new NotSupportedException();
}
