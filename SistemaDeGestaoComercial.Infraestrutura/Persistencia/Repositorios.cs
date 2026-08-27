using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SistemaDeGestaoComercial.Aplicacao.Abstractions;
using SistemaDeGestaoComercial.Dominio.Entidades;

namespace SistemaDeGestaoComercial.Infraestrutura.Persistencia;

internal static class ConsultaPaginada
{
    public static async Task<ResultadoPaginado<T>> ExecutarAsync<T>(
        IQueryable<T> consulta,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken
    )
    {
        var total = await consulta.CountAsync(cancellationToken);
        var itens = await consulta
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(cancellationToken);
        return new(itens, pagina, tamanhoPagina, total);
    }
}

internal sealed class TransacaoAplicacao(IDbContextTransaction transacao) : ITransacaoAplicacao
{
    public Task ConfirmarAsync(CancellationToken cancellationToken) => transacao.CommitAsync(cancellationToken);

    public ValueTask DisposeAsync() => transacao.DisposeAsync();
}

internal sealed class UnidadeTrabalho(AppDbContext contexto) : IUnidadeTrabalho
{
    public async Task<ITransacaoAplicacao> IniciarTransacaoAsync(
        IsolationLevel isolamento,
        CancellationToken cancellationToken
    ) => new TransacaoAplicacao(await contexto.Database.BeginTransactionAsync(isolamento, cancellationToken));

    public async Task SalvarAsync(CancellationToken cancellationToken)
    {
        try
        {
            await contexto.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException excecao)
        {
            throw new ConflitoPersistenciaException(
                "Os dados foram alterados por outro usuário. Atualize a página e tente novamente.",
                excecao
            );
        }
        catch (DbUpdateException excecao)
        {
            if (excecao.InnerException is SqlException { Number: 2601 or 2627 })
                throw new ConflitoPersistenciaException(
                    "Não foi possível salvar porque os dados conflitam com outro registro.",
                    excecao
                );
            throw;
        }
    }
}

internal sealed class UsuarioRepositorio(AppDbContext contexto) : IUsuarioRepositorio
{
    public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken) =>
        contexto.Usuarios.SingleOrDefaultAsync(usuario => usuario.Email == email, cancellationToken);

    public Task<Usuario?> ObterPorIdAsync(Guid usuarioId, CancellationToken cancellationToken) =>
        contexto.Usuarios.SingleOrDefaultAsync(usuario => usuario.Id == usuarioId, cancellationToken);

    public Task<bool> ExisteEmailAsync(string email, Guid? ignorarUsuarioId, CancellationToken cancellationToken) =>
        contexto.Usuarios.AnyAsync(
            usuario => usuario.Email == email && (!ignorarUsuarioId.HasValue || usuario.Id != ignorarUsuarioId),
            cancellationToken
        );

    public Task<ResultadoPaginado<Usuario>> ListarAsync(
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken
    ) =>
        ConsultaPaginada.ExecutarAsync(
            contexto.Usuarios.AsNoTracking().OrderBy(usuario => usuario.Nome),
            pagina,
            tamanhoPagina,
            cancellationToken
        );

    public void Adicionar(Usuario usuario) => contexto.Usuarios.Add(usuario);
}

internal sealed class ClienteRepositorio(AppDbContext contexto) : IClienteRepositorio
{
    public Task<Cliente?> ObterAsync(Guid clienteId, CancellationToken cancellationToken) =>
        contexto.Clientes.SingleOrDefaultAsync(cliente => cliente.Id == clienteId, cancellationToken);

    public Task<ResultadoPaginado<Cliente>> ListarAsync(
        string? busca,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken
    )
    {
        var consulta = contexto.Clientes.AsNoTracking().Where(cliente => cliente.Ativo);
        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            consulta = consulta.Where(cliente =>
                cliente.Nome.Contains(termo) || cliente.CPF.Contains(termo) || cliente.Telefone.Contains(termo)
            );
        }
        return ConsultaPaginada.ExecutarAsync(
            consulta.OrderBy(cliente => cliente.Nome),
            pagina,
            tamanhoPagina,
            cancellationToken
        );
    }

    public Task<bool> ExisteCpfAsync(string cpf, CancellationToken cancellationToken) =>
        contexto.Clientes.AnyAsync(cliente => cliente.CPF == cpf, cancellationToken);

    public Task<bool> ExisteEmailAsync(string email, Guid? ignorarClienteId, CancellationToken cancellationToken) =>
        contexto.Clientes.AnyAsync(
            cliente => cliente.Email == email && (!ignorarClienteId.HasValue || cliente.Id != ignorarClienteId),
            cancellationToken
        );

    public Task<bool> ExisteAtivoAsync(Guid clienteId, CancellationToken cancellationToken) =>
        contexto.Clientes.AnyAsync(cliente => cliente.Id == clienteId && cliente.Ativo, cancellationToken);

    public Task<bool> PossuiVendasAsync(Guid clienteId, CancellationToken cancellationToken) =>
        contexto.Vendas.AnyAsync(venda => venda.ClienteId == clienteId, cancellationToken);

    public void Adicionar(Cliente cliente) => contexto.Clientes.Add(cliente);

    public void Remover(Cliente cliente) => contexto.Clientes.Remove(cliente);
}

internal sealed class ProdutoRepositorio(AppDbContext contexto) : IProdutoRepositorio
{
    public Task<Produto?> ObterAsync(Guid produtoId, CancellationToken cancellationToken) =>
        contexto.Produtos.SingleOrDefaultAsync(produto => produto.Id == produtoId, cancellationToken);

    public Task<Dictionary<Guid, Produto>> ObterAtivosAsync(
        IReadOnlyCollection<Guid> produtoIds,
        CancellationToken cancellationToken
    ) =>
        contexto
            .Produtos.Where(produto => produtoIds.Contains(produto.Id) && produto.Ativo)
            .ToDictionaryAsync(produto => produto.Id, cancellationToken);

    public Task<ResultadoPaginado<Produto>> ListarAsync(
        string? busca,
        bool estoqueBaixo,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken
    )
    {
        var consulta = contexto.Produtos.AsNoTracking().Where(produto => produto.Ativo);
        if (estoqueBaixo)
            consulta = consulta.Where(produto => produto.QuantidadeEstoque <= produto.EstoqueMinimo);
        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            consulta = consulta.Where(produto => produto.Codigo.Contains(termo) || produto.Nome.Contains(termo));
        }
        return ConsultaPaginada.ExecutarAsync(
            consulta.OrderBy(produto => produto.Nome),
            pagina,
            tamanhoPagina,
            cancellationToken
        );
    }

    public Task<bool> ExisteCodigoAsync(string codigo, CancellationToken cancellationToken) =>
        contexto.Produtos.AnyAsync(produto => produto.Codigo == codigo, cancellationToken);

    public Task<bool> PossuiItensVendaAsync(Guid produtoId, CancellationToken cancellationToken) =>
        contexto.ItensVenda.AnyAsync(item => item.ProdutoId == produtoId, cancellationToken);

    public void Adicionar(Produto produto) => contexto.Produtos.Add(produto);

    public void Remover(Produto produto) => contexto.Produtos.Remove(produto);
}

internal sealed class EstoqueRepositorio(AppDbContext contexto) : IEstoqueRepositorio
{
    public Task<ResultadoPaginado<MovimentacaoEstoque>> ListarAsync(
        Guid? produtoId,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken
    )
    {
        IQueryable<MovimentacaoEstoque> consulta = contexto
            .MovimentacoesEstoque.AsNoTracking()
            .Include(movimento => movimento.Produto);
        if (produtoId.HasValue)
            consulta = consulta.Where(movimento => movimento.ProdutoId == produtoId.Value);
        return ConsultaPaginada.ExecutarAsync(
            consulta.OrderByDescending(movimento => movimento.CreatedAt),
            pagina,
            tamanhoPagina,
            cancellationToken
        );
    }

    public void Adicionar(MovimentacaoEstoque movimentacao) => contexto.MovimentacoesEstoque.Add(movimentacao);
}

internal sealed class VendaRepositorio(AppDbContext contexto) : IVendaRepositorio
{
    private static long sequencialAlternativo = DateTime.UtcNow.Ticks;

    public Task<Venda?> ObterAsync(Guid vendaId, bool rastrear, CancellationToken cancellationToken)
    {
        IQueryable<Venda> consulta = contexto.Vendas;
        if (!rastrear)
            consulta = consulta.AsNoTracking();
        return consulta
            .Include(venda => venda.Cliente)
            .Include(venda => venda.Itens)
                .ThenInclude(item => item.Produto)
            .SingleOrDefaultAsync(venda => venda.Id == vendaId, cancellationToken);
    }

    public Task<ResultadoPaginado<Venda>> ListarAsync(
        Guid? clienteId,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken
    )
    {
        var consulta = contexto
            .Vendas.AsNoTracking()
            .Include(venda => venda.Cliente)
            .Include(venda => venda.Itens)
                .ThenInclude(item => item.Produto)
            .AsQueryable();
        if (clienteId.HasValue)
            consulta = consulta.Where(venda => venda.ClienteId == clienteId.Value);
        return ConsultaPaginada.ExecutarAsync(
            consulta.OrderByDescending(venda => venda.DataVenda),
            pagina,
            tamanhoPagina,
            cancellationToken
        );
    }

    public async Task<long> ObterProximoNumeroAsync(CancellationToken cancellationToken)
    {
        if (!contexto.Database.IsSqlServer())
            return Interlocked.Increment(ref sequencialAlternativo);
        var valoresSequence = await contexto
            .Database.SqlQueryRaw<long>("SELECT NEXT VALUE FOR dbo.NumeroVendaSequence AS [Value]")
            .ToListAsync(cancellationToken);
        return valoresSequence.Single();
    }

    public Task<ResultadoIdempotencia?> ObterVendaPorChaveIdempotenciaAsync(
        string chaveIdempotencia,
        CancellationToken cancellationToken
    ) =>
        contexto
            .RegistrosIdempotencia.Where(registro => registro.Chave == chaveIdempotencia)
            .Select(registro => new ResultadoIdempotencia(registro.VendaId, registro.HashRequisicao))
            .SingleOrDefaultAsync(cancellationToken);

    public void RegistrarIdempotencia(
        string chaveIdempotencia,
        string hashRequisicao,
        Guid vendaId,
        string usuarioResponsavel
    ) =>
        contexto.RegistrosIdempotencia.Add(
            new RegistroIdempotencia(chaveIdempotencia, hashRequisicao, vendaId, usuarioResponsavel)
        );

    public void Adicionar(Venda venda) => contexto.Vendas.Add(venda);
}

internal sealed class FinanceiroRepositorio(AppDbContext contexto) : IFinanceiroRepositorio
{
    public Task<ResultadoPaginado<MovimentacaoFinanceira>> ListarAsync(
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken
    ) =>
        ConsultaPaginada.ExecutarAsync(
            contexto.MovimentacoesFinanceiras.AsNoTracking().OrderByDescending(movimento => movimento.DataMovimentacao),
            pagina,
            tamanhoPagina,
            cancellationToken
        );

    public async Task<TotaisFinanceiros> ObterTotaisAsync(
        DateTime inicioDia,
        DateTime inicioMes,
        CancellationToken cancellationToken
    )
    {
        var financeiro = await contexto
            .MovimentacoesFinanceiras.AsNoTracking()
            .Where(movimento => movimento.DataMovimentacao >= inicioMes)
            .GroupBy(_ => 1)
            .Select(grupo => new
            {
                EntradasDia = grupo.Sum(movimento =>
                    movimento.DataMovimentacao >= inicioDia
                    && movimento.TipoMovimentacao == TipoMovimentacaoFinanceira.Entrada
                        ? movimento.Valor
                        : 0
                ),
                EntradasMes = grupo.Sum(movimento =>
                    movimento.TipoMovimentacao == TipoMovimentacaoFinanceira.Entrada ? movimento.Valor : 0
                ),
                SaidasDia = grupo.Sum(movimento =>
                    movimento.DataMovimentacao >= inicioDia
                    && movimento.TipoMovimentacao == TipoMovimentacaoFinanceira.Saida
                        ? movimento.Valor
                        : 0
                ),
                SaidasMes = grupo.Sum(movimento =>
                    movimento.TipoMovimentacao == TipoMovimentacaoFinanceira.Saida ? movimento.Valor : 0
                ),
                EstornosDia = grupo.Sum(movimento =>
                    movimento.DataMovimentacao >= inicioDia
                    && movimento.TipoMovimentacao == TipoMovimentacaoFinanceira.Estorno
                        ? movimento.Valor
                        : 0
                ),
                EstornosMes = grupo.Sum(movimento =>
                    movimento.TipoMovimentacao == TipoMovimentacaoFinanceira.Estorno ? movimento.Valor : 0
                ),
            })
            .SingleOrDefaultAsync(cancellationToken);
        var vendas = await contexto
            .Vendas.AsNoTracking()
            .Where(venda => venda.DataVenda >= inicioMes && venda.Situacao == SituacaoVenda.Concluida)
            .GroupBy(_ => 1)
            .Select(grupo => new { Dia = grupo.Count(venda => venda.DataVenda >= inicioDia), Mes = grupo.Count() })
            .SingleOrDefaultAsync(cancellationToken);
        return new(
            financeiro?.EntradasDia ?? 0,
            financeiro?.EntradasMes ?? 0,
            financeiro?.SaidasDia ?? 0,
            financeiro?.SaidasMes ?? 0,
            financeiro?.EstornosDia ?? 0,
            financeiro?.EstornosMes ?? 0,
            vendas?.Dia ?? 0,
            vendas?.Mes ?? 0
        );
    }

    public void Adicionar(MovimentacaoFinanceira movimentacao) => contexto.MovimentacoesFinanceiras.Add(movimentacao);
}
