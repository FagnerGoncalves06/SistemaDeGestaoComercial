using System.ComponentModel.DataAnnotations;
using SistemaDeGestaoComercial.Aplicacao.Abstractions;
using SistemaDeGestaoComercial.Dominio.Entidades;

namespace SistemaDeGestaoComercial.Aplicacao.Contratos;

public sealed record EnderecoDto(
    [Required, RegularExpression(@"^\d{5}-?\d{3}$")] string Cep,
    [Required, StringLength(LimitesDominio.Logradouro)] string Logradouro,
    [Required, StringLength(LimitesDominio.NumeroEndereco)] string Numero,
    [StringLength(LimitesDominio.ComplementoEndereco)] string? Complemento,
    [Required, StringLength(LimitesDominio.Bairro)] string Bairro,
    [Required, StringLength(LimitesDominio.Cidade)] string Cidade,
    [Required, StringLength(LimitesDominio.Uf, MinimumLength = LimitesDominio.Uf)] string Uf
);

public sealed record ClienteEntrada(
    [Required, StringLength(LimitesDominio.Nome, MinimumLength = 2)] string Nome,
    [Required, RegularExpression(@"^\d{3}\.?\d{3}\.?\d{3}-?\d{2}$")] string Cpf,
    [EmailAddress, StringLength(LimitesDominio.Email)] string? Email,
    [Required, StringLength(LimitesDominio.Telefone)] string Telefone,
    DateOnly? DataNascimento,
    EnderecoDto Endereco
);

public sealed record ClienteAtualizacao(
    [Required, StringLength(LimitesDominio.Nome, MinimumLength = 2)] string Nome,
    [EmailAddress, StringLength(LimitesDominio.Email)] string? Email,
    [Required, StringLength(LimitesDominio.Telefone)] string Telefone,
    DateOnly? DataNascimento,
    EnderecoDto Endereco
);

public sealed record ClienteDto(
    Guid Id,
    string Nome,
    string Cpf,
    string? Email,
    string Telefone,
    DateOnly? DataNascimento,
    EnderecoDto Endereco,
    bool Ativo
);

public sealed record ProdutoEntrada(
    [Required, StringLength(LimitesDominio.CodigoProduto)] string Codigo,
    [Required, StringLength(LimitesDominio.Nome, MinimumLength = 2)] string Nome,
    [StringLength(LimitesDominio.Descricao)] string? Descricao,
    [Range(0, 9999999999999999.99)] decimal PrecoCusto,
    [Range(0, 9999999999999999.99)] decimal PrecoVenda,
    [Range(0, int.MaxValue)] int EstoqueMinimo
);

public sealed record ProdutoAtualizacao(
    [Required, StringLength(LimitesDominio.Nome, MinimumLength = 2)] string Nome,
    [StringLength(LimitesDominio.Descricao)] string? Descricao,
    [Range(0, 9999999999999999.99)] decimal PrecoCusto,
    [Range(0, 9999999999999999.99)] decimal PrecoVenda,
    [Range(0, int.MaxValue)] int EstoqueMinimo
);

public sealed record ProdutoDto(
    Guid Id,
    string Codigo,
    string Nome,
    string? Descricao,
    decimal PrecoCusto,
    decimal PrecoVenda,
    int QuantidadeEstoque,
    int EstoqueMinimo,
    bool Ativo
);

public sealed record EstoqueEntrada(
    Guid ProdutoId,
    [Range(1, int.MaxValue)] int Quantidade,
    [EnumDataType(typeof(TipoMovimentacaoEstoque))] TipoMovimentacaoEstoque Tipo,
    [StringLength(LimitesDominio.Observacao)] string? Observacao
);

public sealed record MovimentoEstoqueDto(
    Guid Id,
    Guid ProdutoId,
    string Produto,
    TipoMovimentacaoEstoque Tipo,
    int Quantidade,
    int Anterior,
    int Posterior,
    DateTime Data,
    string Usuario,
    string? Observacao
);

public sealed record ItemVendaEntrada(
    Guid ProdutoId,
    [Range(1, int.MaxValue)] int Quantidade,
    [Range(0, 9999999999999999.99)] decimal Desconto
);

public sealed record VendaEntrada(
    Guid? ClienteId,
    [Range(0, 9999999999999999.99)] decimal Desconto,
    [EnumDataType(typeof(FormaPagamento))] FormaPagamento FormaPagamento,
    [Required, MinLength(1)] IReadOnlyList<ItemVendaEntrada> Itens
);

public sealed record ItemVendaDto(
    Guid ProdutoId,
    string Produto,
    int Quantidade,
    decimal PrecoUnitario,
    decimal Desconto,
    decimal Total
);

public sealed record VendaDto(
    Guid Id,
    string Numero,
    Guid? ClienteId,
    string? Cliente,
    DateTime DataVenda,
    decimal Subtotal,
    decimal Desconto,
    decimal Total,
    FormaPagamento FormaPagamento,
    SituacaoVenda Situacao,
    IReadOnlyList<ItemVendaDto> Itens
);

public sealed record DespesaEntrada(
    [Required, StringLength(LimitesDominio.Descricao)] string Descricao,
    [Range(0.01, 9999999999999999.99)] decimal Valor
);

public sealed record MovimentoFinanceiroDto(
    Guid Id,
    TipoMovimentacaoFinanceira Tipo,
    string Descricao,
    decimal Valor,
    DateTime Data,
    Guid? VendaId
);

public sealed record DashboardDto(
    decimal FaturamentoDia,
    decimal FaturamentoMes,
    decimal DespesasDia,
    decimal DespesasMes,
    decimal EstornosDia,
    decimal EstornosMes,
    decimal SaldoDia,
    decimal SaldoMes,
    int VendasDia,
    int VendasMes,
    decimal TicketMedioDia,
    decimal TicketMedioMes,
    IReadOnlyList<ProdutoDto> EstoqueBaixo
);

public sealed record LoginEntrada(
    [Required, EmailAddress, StringLength(LimitesDominio.Email)] string Email,
    [Required, StringLength(128, MinimumLength = 8)] string Senha
);

public sealed record LoginDto(string Token, DateTime ExpiraEm, string Nome, PerfilUsuario Perfil);

public sealed record UsuarioEntrada(
    [Required, StringLength(LimitesDominio.Nome, MinimumLength = 2)] string Nome,
    [Required, EmailAddress, StringLength(LimitesDominio.Email)] string Email,
    [Required, StringLength(128, MinimumLength = 8)] string Senha,
    [EnumDataType(typeof(PerfilUsuario))] PerfilUsuario Perfil
);

public sealed record UsuarioAtualizacao(
    [Required, StringLength(LimitesDominio.Nome, MinimumLength = 2)] string Nome,
    [EnumDataType(typeof(PerfilUsuario))] PerfilUsuario Perfil,
    bool Ativo
);

public sealed record TrocaSenhaEntrada([Required, StringLength(128, MinimumLength = 8)] string NovaSenha);

public sealed record UsuarioDto(Guid Id, string Nome, string Email, PerfilUsuario Perfil, bool Ativo);

public sealed record ReciboDto(
    string Numero,
    DateTime Data,
    string? Cliente,
    IReadOnlyList<ItemVendaDto> Produtos,
    decimal Subtotal,
    decimal Desconto,
    decimal Total,
    FormaPagamento FormaPagamento
);

public interface IAutenticacaoService
{
    Task<LoginDto> LoginAsync(LoginEntrada entrada, CancellationToken cancellationToken);
}

public interface IUsuarioService
{
    Task<Pagina<UsuarioDto>> ListarUsuariosAsync(int pagina, int tamanhoPagina, CancellationToken cancellationToken);
    Task<UsuarioDto> CriarUsuarioAsync(
        UsuarioEntrada entrada,
        string usuarioResponsavel,
        CancellationToken cancellationToken
    );
    Task<UsuarioDto> AtualizarUsuarioAsync(
        Guid usuarioId,
        UsuarioAtualizacao entrada,
        string usuarioResponsavel,
        CancellationToken cancellationToken
    );
    Task TrocarSenhaAsync(
        Guid usuarioId,
        string novaSenha,
        string usuarioResponsavel,
        CancellationToken cancellationToken
    );
}

public interface IClienteService
{
    Task<Pagina<ClienteDto>> ListarClientesAsync(
        string? busca,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken
    );
    Task<ClienteDto> ObterClienteAsync(Guid clienteId, CancellationToken cancellationToken);
    Task<ClienteDto> CriarClienteAsync(
        ClienteEntrada entrada,
        string usuarioResponsavel,
        CancellationToken cancellationToken
    );
    Task<ClienteDto> AtualizarClienteAsync(
        Guid clienteId,
        ClienteAtualizacao entrada,
        string usuarioResponsavel,
        CancellationToken cancellationToken
    );
    Task ExcluirClienteAsync(Guid clienteId, string usuarioResponsavel, CancellationToken cancellationToken);
    Task<Pagina<VendaDto>> HistoricoClienteAsync(
        Guid clienteId,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken
    );
}

public interface IProdutoService
{
    Task<Pagina<ProdutoDto>> ListarProdutosAsync(
        string? busca,
        int pagina,
        int tamanhoPagina,
        bool estoqueBaixo,
        CancellationToken cancellationToken
    );
    Task<ProdutoDto> ObterProdutoAsync(Guid produtoId, CancellationToken cancellationToken);
    Task<ProdutoDto> CriarProdutoAsync(
        ProdutoEntrada entrada,
        string usuarioResponsavel,
        CancellationToken cancellationToken
    );
    Task<ProdutoDto> AtualizarProdutoAsync(
        Guid produtoId,
        ProdutoAtualizacao entrada,
        string usuarioResponsavel,
        CancellationToken cancellationToken
    );
    Task ExcluirProdutoAsync(Guid produtoId, string usuarioResponsavel, CancellationToken cancellationToken);
}

public interface IEstoqueService
{
    Task<MovimentoEstoqueDto> MovimentarEstoqueAsync(
        EstoqueEntrada entrada,
        string usuarioResponsavel,
        CancellationToken cancellationToken
    );
    Task<Pagina<MovimentoEstoqueDto>> ListarMovimentosAsync(
        Guid? produtoId,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken
    );
}

public interface IVendaService
{
    Task<VendaDto> CriarVendaAsync(
        VendaEntrada entrada,
        string chaveIdempotencia,
        string usuarioResponsavel,
        CancellationToken cancellationToken
    );
    Task<Pagina<VendaDto>> ListarVendasAsync(int pagina, int tamanhoPagina, CancellationToken cancellationToken);
    Task<VendaDto> ObterVendaAsync(Guid vendaId, CancellationToken cancellationToken);
    Task<VendaDto> CancelarVendaAsync(Guid vendaId, string usuarioResponsavel, CancellationToken cancellationToken);
    Task<ReciboDto> ObterReciboAsync(Guid vendaId, CancellationToken cancellationToken);
}

public interface IFinanceiroService
{
    Task<MovimentoFinanceiroDto> CriarDespesaAsync(
        DespesaEntrada entrada,
        string usuarioResponsavel,
        CancellationToken cancellationToken
    );
    Task<Pagina<MovimentoFinanceiroDto>> ListarFinanceiroAsync(
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken
    );
}

public interface IDashboardService
{
    Task<DashboardDto> ObterAsync(CancellationToken cancellationToken);
}

public interface IValidacaoSessaoService
{
    Task<bool> UsuarioPodeAcessarAsync(
        Guid usuarioId,
        int versaoToken,
        PerfilUsuario perfil,
        CancellationToken cancellationToken
    );
}

public interface ITokenService
{
    LoginDto Criar(Usuario usuario);
}

public interface ISenhaService
{
    string Hash(string senha);
    bool Verificar(string senha, string hash);
}
