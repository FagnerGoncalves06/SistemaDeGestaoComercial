using System.Data;
using SistemaDeGestaoComercial.Aplicacao.Contratos;
using SistemaDeGestaoComercial.Dominio.Entidades;

namespace SistemaDeGestaoComercial.Aplicacao.Abstractions;

public sealed record ResultadoPaginado<T>(IReadOnlyList<T> Itens, int Pagina, int TamanhoPagina, int TotalItens);

public sealed record ResultadoIdempotencia(Guid VendaId, string HashRequisicao);

public sealed record TotaisFinanceiros(
    decimal EntradasDia,
    decimal EntradasMes,
    decimal SaidasDia,
    decimal SaidasMes,
    decimal EstornosDia,
    decimal EstornosMes,
    int VendasDia,
    int VendasMes
);

public interface ITransacaoAplicacao : IAsyncDisposable
{
    Task ConfirmarAsync(CancellationToken cancellationToken);
}

public interface IUnidadeTrabalho
{
    Task<ITransacaoAplicacao> IniciarTransacaoAsync(IsolationLevel isolamento, CancellationToken cancellationToken);
    Task SalvarAsync(CancellationToken cancellationToken);
}

public interface IOutboxRepositorio
{
    void Adicionar(VendaRealizadaEvent evento, string? correlationId = null);
}

public interface IInboxRepositorio
{
    Task<bool> JaProcessadaAsync(Guid messageId, string consumer, CancellationToken cancellationToken);
    void Adicionar(Guid messageId, string consumer);
}

public interface IAlertaEstoqueRepositorio
{
    Task<ResultadoPaginado<AlertaEstoque>> ListarAsync(
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken
    );
    Task<AlertaEstoque?> ObterAsync(Guid id, CancellationToken cancellationToken);
}

public interface IUsuarioRepositorio
{
    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken);
    Task<Usuario?> ObterPorIdAsync(Guid usuarioId, CancellationToken cancellationToken);
    Task<bool> ExisteEmailAsync(string email, Guid? ignorarUsuarioId, CancellationToken cancellationToken);
    Task<ResultadoPaginado<Usuario>> ListarAsync(int pagina, int tamanhoPagina, CancellationToken cancellationToken);
    void Adicionar(Usuario usuario);
}

public interface IClienteRepositorio
{
    Task<Cliente?> ObterAsync(Guid clienteId, CancellationToken cancellationToken);
    Task<ResultadoPaginado<Cliente>> ListarAsync(
        string? busca,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken
    );
    Task<bool> ExisteCpfAsync(string cpf, CancellationToken cancellationToken);
    Task<bool> ExisteEmailAsync(string email, Guid? ignorarClienteId, CancellationToken cancellationToken);
    Task<bool> ExisteAtivoAsync(Guid clienteId, CancellationToken cancellationToken);
    Task<bool> PossuiVendasAsync(Guid clienteId, CancellationToken cancellationToken);
    void Adicionar(Cliente cliente);
    void Remover(Cliente cliente);
}

public interface IProdutoRepositorio
{
    Task<Produto?> ObterAsync(Guid produtoId, CancellationToken cancellationToken);
    Task<Dictionary<Guid, Produto>> ObterAtivosAsync(
        IReadOnlyCollection<Guid> produtoIds,
        CancellationToken cancellationToken
    );
    Task<ResultadoPaginado<Produto>> ListarAsync(
        string? busca,
        bool estoqueBaixo,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken
    );
    Task<bool> ExisteCodigoAsync(string codigo, CancellationToken cancellationToken);
    Task<bool> PossuiItensVendaAsync(Guid produtoId, CancellationToken cancellationToken);
    void Adicionar(Produto produto);
    void Remover(Produto produto);
}

public interface IEstoqueRepositorio
{
    Task<ResultadoPaginado<MovimentacaoEstoque>> ListarAsync(
        Guid? produtoId,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken
    );
    void Adicionar(MovimentacaoEstoque movimentacao);
}

public interface IVendaRepositorio
{
    Task<Venda?> ObterAsync(Guid vendaId, bool rastrear, CancellationToken cancellationToken);
    Task<ResultadoPaginado<Venda>> ListarAsync(
        Guid? clienteId,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken
    );
    Task<long> ObterProximoNumeroAsync(CancellationToken cancellationToken);
    Task<ResultadoIdempotencia?> ObterVendaPorChaveIdempotenciaAsync(
        string chaveIdempotencia,
        CancellationToken cancellationToken
    );
    void RegistrarIdempotencia(
        string chaveIdempotencia,
        string hashRequisicao,
        Guid vendaId,
        string usuarioResponsavel
    );
    void Adicionar(Venda venda);
}

public interface IFinanceiroRepositorio
{
    Task<ResultadoPaginado<MovimentacaoFinanceira>> ListarAsync(
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken
    );
    Task<TotaisFinanceiros> ObterTotaisAsync(
        DateTime inicioDia,
        DateTime inicioMes,
        CancellationToken cancellationToken
    );
    void Adicionar(MovimentacaoFinanceira movimentacao);
}

public sealed class ConflitoPersistenciaException(string mensagem, Exception? innerException = null)
    : Exception(mensagem, innerException);

public sealed class CredenciaisInvalidasException() : Exception("Credenciais inválidas.");

public interface IRelogioNegocio
{
    DateTime UtcAgora { get; }
    (DateTime InicioDiaUtc, DateTime InicioMesUtc) ObterLimitesUtc();
}

public interface ICacheSessao
{
    Task<bool?> ObterAsync(Guid usuarioId, int versaoToken, PerfilUsuario perfil, CancellationToken cancellationToken);
    Task ArmazenarAsync(
        Guid usuarioId,
        int versaoToken,
        PerfilUsuario perfil,
        bool permitido,
        CancellationToken cancellationToken
    );
    Task InvalidarAsync(Guid usuarioId, CancellationToken cancellationToken);
}
