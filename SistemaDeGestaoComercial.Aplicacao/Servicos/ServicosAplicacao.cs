using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SistemaDeGestaoComercial.Aplicacao.Abstractions;
using SistemaDeGestaoComercial.Aplicacao.Contratos;
using SistemaDeGestaoComercial.Dominio.Entidades;

namespace SistemaDeGestaoComercial.Aplicacao.Servicos;

internal static class Paginacao
{
    public static (int Pagina, int TamanhoPagina) Normalizar(int pagina, int tamanhoPagina) =>
        (Math.Max(1, pagina), Math.Clamp(tamanhoPagina, 1, 100));

    public static Pagina<TDto> Mapear<TEntidade, TDto>(
        ResultadoPaginado<TEntidade> resultado,
        Func<TEntidade, TDto> mapear
    ) => new(resultado.Itens.Select(mapear).ToList(), resultado.Pagina, resultado.TamanhoPagina, resultado.TotalItens);
}

internal static class Mapeamento
{
    public static Endereco ParaEntidade(EnderecoDto endereco) =>
        new(
            endereco.Cep,
            endereco.Logradouro,
            endereco.Numero,
            endereco.Complemento,
            endereco.Bairro,
            endereco.Cidade,
            endereco.Uf
        );

    public static EnderecoDto ParaDto(Endereco endereco) =>
        new(
            endereco.CEP,
            endereco.Logradouro,
            endereco.Numero,
            endereco.Complemento,
            endereco.Bairro,
            endereco.Cidade,
            endereco.UF
        );

    public static UsuarioDto ParaDto(Usuario usuario) =>
        new(usuario.Id, usuario.Nome, usuario.Email, usuario.Perfil, usuario.Ativo);

    public static ClienteDto ParaDto(Cliente cliente) =>
        new(
            cliente.Id,
            cliente.Nome,
            cliente.CPF,
            cliente.Email,
            cliente.Telefone,
            cliente.DataNascimento,
            ParaDto(cliente.Endereco),
            cliente.Ativo
        );

    public static ProdutoDto ParaDto(Produto produto) =>
        new(
            produto.Id,
            produto.Codigo,
            produto.Nome,
            produto.Descricao,
            produto.PrecoCusto,
            produto.PrecoVenda,
            produto.QuantidadeEstoque,
            produto.EstoqueMinimo,
            produto.Ativo
        );

    public static MovimentoEstoqueDto ParaDto(MovimentacaoEstoque movimento) =>
        new(
            movimento.Id,
            movimento.ProdutoId,
            movimento.Produto.Nome,
            movimento.TipoMovimentacao,
            movimento.Quantidade,
            movimento.QuantidadeAnterior,
            movimento.QuantidadePosterior,
            movimento.CreatedAt,
            movimento.CriadoPor,
            movimento.Observacao
        );

    public static MovimentoEstoqueDto ParaDto(MovimentacaoEstoque movimento, string nomeProduto) =>
        new(
            movimento.Id,
            movimento.ProdutoId,
            nomeProduto,
            movimento.TipoMovimentacao,
            movimento.Quantidade,
            movimento.QuantidadeAnterior,
            movimento.QuantidadePosterior,
            movimento.CreatedAt,
            movimento.CriadoPor,
            movimento.Observacao
        );

    public static MovimentoFinanceiroDto ParaDto(MovimentacaoFinanceira movimento) =>
        new(
            movimento.Id,
            movimento.TipoMovimentacao,
            movimento.Descricao,
            movimento.Valor,
            movimento.DataMovimentacao,
            movimento.VendaId
        );

    public static VendaDto ParaDto(Venda venda) =>
        new(
            venda.Id,
            venda.Numero,
            venda.ClienteId,
            venda.Cliente?.Nome,
            venda.DataVenda,
            venda.Subtotal,
            venda.Desconto,
            venda.Total,
            venda.FormaPagamento,
            venda.Situacao,
            venda
                .Itens.Select(item => new ItemVendaDto(
                    item.ProdutoId,
                    item.Produto.Nome,
                    item.Quantidade,
                    item.PrecoUnitario,
                    item.Desconto,
                    item.Total
                ))
                .ToList()
        );
}

public sealed class AutenticacaoService(
    IUsuarioRepositorio usuarios,
    ISenhaService senhaService,
    ITokenService tokenService
) : IAutenticacaoService
{
    public async Task<LoginDto> LoginAsync(LoginEntrada entrada, CancellationToken cancellationToken)
    {
        var email = entrada.Email.Trim().ToLowerInvariant();
        var usuario = await usuarios.ObterPorEmailAsync(email, cancellationToken);
        if (usuario is null || !usuario.Ativo || !senhaService.Verificar(entrada.Senha, usuario.SenhaHash))
            throw new CredenciaisInvalidasException();
        return tokenService.Criar(usuario);
    }
}

public sealed class ValidacaoSessaoService(IUsuarioRepositorio usuarios, ICacheSessao cacheSessao)
    : IValidacaoSessaoService
{
    public async Task<bool> UsuarioPodeAcessarAsync(
        Guid usuarioId,
        int versaoToken,
        PerfilUsuario perfil,
        CancellationToken cancellationToken
    )
    {
        var resultadoCache = await cacheSessao.ObterAsync(usuarioId, versaoToken, perfil, cancellationToken);
        if (resultadoCache.HasValue)
            return resultadoCache.Value;
        var usuario = await usuarios.ObterPorIdAsync(usuarioId, cancellationToken);
        var permitido =
            usuario is not null && usuario.Ativo && usuario.VersaoToken == versaoToken && usuario.Perfil == perfil;
        await cacheSessao.ArmazenarAsync(usuarioId, versaoToken, perfil, permitido, cancellationToken);
        return permitido;
    }
}

public sealed class UsuarioService(
    IUsuarioRepositorio usuarios,
    IUnidadeTrabalho unidadeTrabalho,
    ISenhaService senhaService,
    ICacheSessao cacheSessao
) : IUsuarioService
{
    public async Task<Pagina<UsuarioDto>> ListarUsuariosAsync(
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken
    )
    {
        (pagina, tamanhoPagina) = Paginacao.Normalizar(pagina, tamanhoPagina);
        var resultado = await usuarios.ListarAsync(pagina, tamanhoPagina, cancellationToken);
        return Paginacao.Mapear(resultado, Mapeamento.ParaDto);
    }

    public async Task<UsuarioDto> CriarUsuarioAsync(
        UsuarioEntrada entrada,
        string usuarioResponsavel,
        CancellationToken cancellationToken
    )
    {
        var email = entrada.Email.Trim().ToLowerInvariant();
        if (await usuarios.ExisteEmailAsync(email, null, cancellationToken))
            throw new ConflitoPersistenciaException("Email já cadastrado.");
        var usuario = new Usuario(
            entrada.Nome,
            email,
            senhaService.Hash(entrada.Senha),
            entrada.Perfil,
            usuarioResponsavel
        );
        usuarios.Adicionar(usuario);
        await unidadeTrabalho.SalvarAsync(cancellationToken);
        return Mapeamento.ParaDto(usuario);
    }

    public async Task<UsuarioDto> AtualizarUsuarioAsync(
        Guid usuarioId,
        UsuarioAtualizacao entrada,
        string usuarioResponsavel,
        CancellationToken cancellationToken
    )
    {
        var usuario =
            await usuarios.ObterPorIdAsync(usuarioId, cancellationToken)
            ?? throw new EntidadeNaoEncontradaException("Usuário não encontrado.");
        usuario.Atualizar(entrada.Nome, entrada.Perfil, entrada.Ativo, usuarioResponsavel);
        await unidadeTrabalho.SalvarAsync(cancellationToken);
        await cacheSessao.InvalidarAsync(usuarioId, cancellationToken);
        return Mapeamento.ParaDto(usuario);
    }

    public async Task TrocarSenhaAsync(
        Guid usuarioId,
        string novaSenha,
        string usuarioResponsavel,
        CancellationToken cancellationToken
    )
    {
        var usuario =
            await usuarios.ObterPorIdAsync(usuarioId, cancellationToken)
            ?? throw new EntidadeNaoEncontradaException("Usuário não encontrado.");
        usuario.TrocarSenha(senhaService.Hash(novaSenha), usuarioResponsavel);
        await unidadeTrabalho.SalvarAsync(cancellationToken);
        await cacheSessao.InvalidarAsync(usuarioId, cancellationToken);
    }
}

public sealed class ClienteService(
    IClienteRepositorio clientes,
    IVendaRepositorio vendas,
    IUnidadeTrabalho unidadeTrabalho
) : IClienteService
{
    public async Task<Pagina<ClienteDto>> ListarClientesAsync(
        string? busca,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken
    )
    {
        (pagina, tamanhoPagina) = Paginacao.Normalizar(pagina, tamanhoPagina);
        var resultado = await clientes.ListarAsync(busca, pagina, tamanhoPagina, cancellationToken);
        return Paginacao.Mapear(resultado, Mapeamento.ParaDto);
    }

    public async Task<ClienteDto> ObterClienteAsync(Guid clienteId, CancellationToken cancellationToken) =>
        Mapeamento.ParaDto(
            await clientes.ObterAsync(clienteId, cancellationToken)
                ?? throw new EntidadeNaoEncontradaException("Cliente não encontrado.")
        );

    public async Task<ClienteDto> CriarClienteAsync(
        ClienteEntrada entrada,
        string usuarioResponsavel,
        CancellationToken cancellationToken
    )
    {
        var cpf = SomenteDigitos(entrada.Cpf);
        if (await clientes.ExisteCpfAsync(cpf, cancellationToken))
            throw new ConflitoPersistenciaException("CPF já cadastrado.");
        var email = entrada.Email?.Trim().ToLowerInvariant();
        if (email is not null && await clientes.ExisteEmailAsync(email, null, cancellationToken))
            throw new ConflitoPersistenciaException("Email já cadastrado.");
        var cliente = new Cliente(
            entrada.Nome,
            entrada.Cpf,
            entrada.Email,
            entrada.Telefone,
            entrada.DataNascimento,
            Mapeamento.ParaEntidade(entrada.Endereco),
            usuarioResponsavel
        );
        clientes.Adicionar(cliente);
        await unidadeTrabalho.SalvarAsync(cancellationToken);
        return Mapeamento.ParaDto(cliente);
    }

    public async Task<ClienteDto> AtualizarClienteAsync(
        Guid clienteId,
        ClienteAtualizacao entrada,
        string usuarioResponsavel,
        CancellationToken cancellationToken
    )
    {
        var cliente =
            await clientes.ObterAsync(clienteId, cancellationToken)
            ?? throw new EntidadeNaoEncontradaException("Cliente não encontrado.");
        var email = entrada.Email?.Trim().ToLowerInvariant();
        if (email is not null && await clientes.ExisteEmailAsync(email, clienteId, cancellationToken))
            throw new ConflitoPersistenciaException("Email já cadastrado.");
        cliente.Atualizar(
            entrada.Nome,
            entrada.Email,
            entrada.Telefone,
            entrada.DataNascimento,
            Mapeamento.ParaEntidade(entrada.Endereco),
            usuarioResponsavel
        );
        await unidadeTrabalho.SalvarAsync(cancellationToken);
        return Mapeamento.ParaDto(cliente);
    }

    public async Task ExcluirClienteAsync(
        Guid clienteId,
        string usuarioResponsavel,
        CancellationToken cancellationToken
    )
    {
        var cliente =
            await clientes.ObterAsync(clienteId, cancellationToken)
            ?? throw new EntidadeNaoEncontradaException("Cliente não encontrado.");
        if (await clientes.PossuiVendasAsync(clienteId, cancellationToken))
            cliente.Inativar(usuarioResponsavel);
        else
            clientes.Remover(cliente);
        await unidadeTrabalho.SalvarAsync(cancellationToken);
    }

    public async Task<Pagina<VendaDto>> HistoricoClienteAsync(
        Guid clienteId,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken
    )
    {
        if (await clientes.ObterAsync(clienteId, cancellationToken) is null)
            throw new EntidadeNaoEncontradaException("Cliente não encontrado.");
        (pagina, tamanhoPagina) = Paginacao.Normalizar(pagina, tamanhoPagina);
        var resultado = await vendas.ListarAsync(clienteId, pagina, tamanhoPagina, cancellationToken);
        return Paginacao.Mapear(resultado, Mapeamento.ParaDto);
    }

    private static string SomenteDigitos(string valor) => new(valor.Where(char.IsDigit).ToArray());
}

public sealed class ProdutoService(IProdutoRepositorio produtos, IUnidadeTrabalho unidadeTrabalho) : IProdutoService
{
    public async Task<Pagina<ProdutoDto>> ListarProdutosAsync(
        string? busca,
        int pagina,
        int tamanhoPagina,
        bool estoqueBaixo,
        CancellationToken cancellationToken
    )
    {
        (pagina, tamanhoPagina) = Paginacao.Normalizar(pagina, tamanhoPagina);
        var resultado = await produtos.ListarAsync(busca, estoqueBaixo, pagina, tamanhoPagina, cancellationToken);
        return Paginacao.Mapear(resultado, Mapeamento.ParaDto);
    }

    public async Task<ProdutoDto> ObterProdutoAsync(Guid produtoId, CancellationToken cancellationToken) =>
        Mapeamento.ParaDto(
            await produtos.ObterAsync(produtoId, cancellationToken)
                ?? throw new EntidadeNaoEncontradaException("Produto não encontrado.")
        );

    public async Task<ProdutoDto> CriarProdutoAsync(
        ProdutoEntrada entrada,
        string usuarioResponsavel,
        CancellationToken cancellationToken
    )
    {
        var codigo = entrada.Codigo.Trim();
        if (await produtos.ExisteCodigoAsync(codigo, cancellationToken))
            throw new ConflitoPersistenciaException("Código já cadastrado.");
        var produto = new Produto(
            codigo,
            entrada.Nome,
            entrada.Descricao,
            entrada.PrecoCusto,
            entrada.PrecoVenda,
            entrada.EstoqueMinimo,
            usuarioResponsavel
        );
        produtos.Adicionar(produto);
        await unidadeTrabalho.SalvarAsync(cancellationToken);
        return Mapeamento.ParaDto(produto);
    }

    public async Task<ProdutoDto> AtualizarProdutoAsync(
        Guid produtoId,
        ProdutoAtualizacao entrada,
        string usuarioResponsavel,
        CancellationToken cancellationToken
    )
    {
        var produto =
            await produtos.ObterAsync(produtoId, cancellationToken)
            ?? throw new EntidadeNaoEncontradaException("Produto não encontrado.");
        produto.Atualizar(
            entrada.Nome,
            entrada.Descricao,
            entrada.PrecoCusto,
            entrada.PrecoVenda,
            entrada.EstoqueMinimo,
            usuarioResponsavel
        );
        await unidadeTrabalho.SalvarAsync(cancellationToken);
        return Mapeamento.ParaDto(produto);
    }

    public async Task ExcluirProdutoAsync(
        Guid produtoId,
        string usuarioResponsavel,
        CancellationToken cancellationToken
    )
    {
        var produto =
            await produtos.ObterAsync(produtoId, cancellationToken)
            ?? throw new EntidadeNaoEncontradaException("Produto não encontrado.");
        if (await produtos.PossuiItensVendaAsync(produtoId, cancellationToken))
            produto.Inativar(usuarioResponsavel);
        else
            produtos.Remover(produto);
        await unidadeTrabalho.SalvarAsync(cancellationToken);
    }
}

public sealed class EstoqueService(
    IProdutoRepositorio produtos,
    IEstoqueRepositorio estoque,
    IUnidadeTrabalho unidadeTrabalho
) : IEstoqueService
{
    public async Task<MovimentoEstoqueDto> MovimentarEstoqueAsync(
        EstoqueEntrada entrada,
        string usuarioResponsavel,
        CancellationToken cancellationToken
    )
    {
        if (entrada.Tipo is TipoMovimentacaoEstoque.Venda or TipoMovimentacaoEstoque.Devolucao)
            throw new ExcecaoDominio("Tipo reservado a vendas.");
        await using var transacao = await unidadeTrabalho.IniciarTransacaoAsync(
            IsolationLevel.Serializable,
            cancellationToken
        );
        var produto =
            await produtos.ObterAsync(entrada.ProdutoId, cancellationToken)
            ?? throw new EntidadeNaoEncontradaException("Produto não encontrado.");
        var movimento = produto.Movimentar(
            entrada.Tipo,
            entrada.Quantidade,
            null,
            entrada.Observacao,
            usuarioResponsavel
        );
        estoque.Adicionar(movimento);
        await unidadeTrabalho.SalvarAsync(cancellationToken);
        await transacao.ConfirmarAsync(cancellationToken);
        return Mapeamento.ParaDto(movimento, produto.Nome);
    }

    public async Task<Pagina<MovimentoEstoqueDto>> ListarMovimentosAsync(
        Guid? produtoId,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken
    )
    {
        (pagina, tamanhoPagina) = Paginacao.Normalizar(pagina, tamanhoPagina);
        var resultado = await estoque.ListarAsync(produtoId, pagina, tamanhoPagina, cancellationToken);
        return Paginacao.Mapear(resultado, Mapeamento.ParaDto);
    }
}

public sealed class VendaService(
    IVendaRepositorio vendas,
    IClienteRepositorio clientes,
    IProdutoRepositorio produtos,
    IEstoqueRepositorio estoque,
    IFinanceiroRepositorio financeiro,
    IOutboxRepositorio outbox,
    IUnidadeTrabalho unidadeTrabalho
) : IVendaService
{
    public async Task<VendaDto> CriarVendaAsync(
        VendaEntrada entrada,
        string chaveIdempotencia,
        string usuarioResponsavel,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(chaveIdempotencia) || chaveIdempotencia.Length > 100)
            throw new ExcecaoDominio("Informe uma chave de idempotência válida.");
        chaveIdempotencia = chaveIdempotencia.Trim();
        var hashRequisicao = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(entrada)))
        );
        if (entrada.Itens.Count == 0)
            throw new ExcecaoDominio("Venda deve possuir itens.");
        await using var transacao = await unidadeTrabalho.IniciarTransacaoAsync(
            IsolationLevel.Serializable,
            cancellationToken
        );
        var resultadoIdempotencia = await vendas.ObterVendaPorChaveIdempotenciaAsync(
            chaveIdempotencia,
            cancellationToken
        );
        if (resultadoIdempotencia is not null)
        {
            if (
                !CryptographicOperations.FixedTimeEquals(
                    Convert.FromHexString(resultadoIdempotencia.HashRequisicao),
                    Convert.FromHexString(hashRequisicao)
                )
            )
                throw new ConflitoPersistenciaException("A chave de idempotência já foi usada com outra venda.");
            return await ObterVendaAsync(resultadoIdempotencia.VendaId, cancellationToken);
        }
        if (entrada.ClienteId.HasValue && !await clientes.ExisteAtivoAsync(entrada.ClienteId.Value, cancellationToken))
            throw new EntidadeNaoEncontradaException("Cliente não encontrado.");
        var produtoIds = entrada.Itens.Select(item => item.ProdutoId).Distinct().ToArray();
        if (produtoIds.Length != entrada.Itens.Count)
            throw new ExcecaoDominio("Produto duplicado na venda.");
        var produtosVenda = await produtos.ObterAtivosAsync(produtoIds, cancellationToken);
        if (produtosVenda.Count != produtoIds.Length)
            throw new EntidadeNaoEncontradaException("Produto não encontrado.");
        var itens = entrada
            .Itens.Select(item => new ItemVenda(
                item.ProdutoId,
                item.Quantidade,
                produtosVenda[item.ProdutoId].PrecoVenda,
                item.Desconto
            ))
            .ToList();
        var sequencial = await vendas.ObterProximoNumeroAsync(cancellationToken);
        var numero = $"V{sequencial:D12}";
        var venda = new Venda(
            numero,
            entrada.ClienteId,
            entrada.FormaPagamento,
            entrada.Desconto,
            itens,
            usuarioResponsavel
        );
        vendas.Adicionar(venda);
        vendas.RegistrarIdempotencia(chaveIdempotencia, hashRequisicao, venda.Id, usuarioResponsavel);
        foreach (var item in entrada.Itens)
        {
            var movimento = produtosVenda[item.ProdutoId]
                .Movimentar(
                    TipoMovimentacaoEstoque.Venda,
                    item.Quantidade,
                    venda.Id,
                    $"Venda {numero}",
                    usuarioResponsavel
                );
            estoque.Adicionar(movimento);
        }
        financeiro.Adicionar(
            new MovimentacaoFinanceira(
                TipoMovimentacaoFinanceira.Entrada,
                $"Venda {numero}",
                venda.Total,
                venda.Id,
                usuarioResponsavel
            )
        );
        outbox.Adicionar(
            new VendaRealizadaEvent(
                Guid.NewGuid(),
                venda.Id,
                venda.Numero,
                venda.ClienteId,
                venda.Total,
                venda.DataVenda,
                venda.Itens.Select(item => new ItemVendaRealizadaEvent(item.ProdutoId, item.Quantidade)).ToArray()
            )
        );
        await unidadeTrabalho.SalvarAsync(cancellationToken);
        await transacao.ConfirmarAsync(cancellationToken);
        return await ObterVendaAsync(venda.Id, cancellationToken);
    }

    public async Task<Pagina<VendaDto>> ListarVendasAsync(
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken
    )
    {
        (pagina, tamanhoPagina) = Paginacao.Normalizar(pagina, tamanhoPagina);
        var resultado = await vendas.ListarAsync(null, pagina, tamanhoPagina, cancellationToken);
        return Paginacao.Mapear(resultado, Mapeamento.ParaDto);
    }

    public async Task<VendaDto> ObterVendaAsync(Guid vendaId, CancellationToken cancellationToken) =>
        Mapeamento.ParaDto(
            await vendas.ObterAsync(vendaId, false, cancellationToken)
                ?? throw new EntidadeNaoEncontradaException("Venda não encontrada.")
        );

    public async Task<VendaDto> CancelarVendaAsync(
        Guid vendaId,
        string usuarioResponsavel,
        CancellationToken cancellationToken
    )
    {
        await using var transacao = await unidadeTrabalho.IniciarTransacaoAsync(
            IsolationLevel.Serializable,
            cancellationToken
        );
        var venda =
            await vendas.ObterAsync(vendaId, true, cancellationToken)
            ?? throw new EntidadeNaoEncontradaException("Venda não encontrada.");
        venda.Cancelar();
        foreach (var item in venda.Itens)
            estoque.Adicionar(
                item.Produto.Movimentar(
                    TipoMovimentacaoEstoque.Devolucao,
                    item.Quantidade,
                    venda.Id,
                    $"Cancelamento {venda.Numero}",
                    usuarioResponsavel
                )
            );
        financeiro.Adicionar(
            new MovimentacaoFinanceira(
                TipoMovimentacaoFinanceira.Estorno,
                $"Estorno {venda.Numero}",
                venda.Total,
                venda.Id,
                usuarioResponsavel
            )
        );
        await unidadeTrabalho.SalvarAsync(cancellationToken);
        await transacao.ConfirmarAsync(cancellationToken);
        return await ObterVendaAsync(vendaId, cancellationToken);
    }

    public async Task<ReciboDto> ObterReciboAsync(Guid vendaId, CancellationToken cancellationToken)
    {
        var venda = await ObterVendaAsync(vendaId, cancellationToken);
        return new(
            venda.Numero,
            venda.DataVenda,
            venda.Cliente,
            venda.Itens,
            venda.Subtotal,
            venda.Desconto,
            venda.Total,
            venda.FormaPagamento
        );
    }
}

public sealed class AlertaEstoqueService(IAlertaEstoqueRepositorio alertas, IUnidadeTrabalho unidadeTrabalho)
    : IAlertaEstoqueService
{
    public async Task<Pagina<AlertaEstoqueDto>> ListarAsync(
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken
    )
    {
        (pagina, tamanhoPagina) = Paginacao.Normalizar(pagina, tamanhoPagina);
        var resultado = await alertas.ListarAsync(pagina, tamanhoPagina, cancellationToken);
        return new Pagina<AlertaEstoqueDto>(
            resultado.Itens.Select(Mapear).ToArray(),
            resultado.Pagina,
            resultado.TamanhoPagina,
            resultado.TotalItens
        );
    }

    public async Task VisualizarAsync(Guid id, string usuarioResponsavel, CancellationToken cancellationToken)
    {
        var alerta =
            await alertas.ObterAsync(id, cancellationToken)
            ?? throw new EntidadeNaoEncontradaException("Alerta de estoque não encontrado.");
        alerta.Visualizar();
        await unidadeTrabalho.SalvarAsync(cancellationToken);
    }

    private static AlertaEstoqueDto Mapear(AlertaEstoque alerta) =>
        new(
            alerta.Id,
            alerta.ProdutoId,
            alerta.Produto.Nome,
            alerta.VendaId,
            alerta.NumeroVenda,
            alerta.QuantidadeAtual,
            alerta.EstoqueMinimo,
            alerta.CreatedAt,
            alerta.Visualizado
        );
}

public sealed class FinanceiroService(IFinanceiroRepositorio financeiro, IUnidadeTrabalho unidadeTrabalho)
    : IFinanceiroService
{
    public async Task<MovimentoFinanceiroDto> CriarDespesaAsync(
        DespesaEntrada entrada,
        string usuarioResponsavel,
        CancellationToken cancellationToken
    )
    {
        var movimento = new MovimentacaoFinanceira(
            TipoMovimentacaoFinanceira.Saida,
            entrada.Descricao,
            entrada.Valor,
            null,
            usuarioResponsavel
        );
        financeiro.Adicionar(movimento);
        await unidadeTrabalho.SalvarAsync(cancellationToken);
        return Mapeamento.ParaDto(movimento);
    }

    public async Task<Pagina<MovimentoFinanceiroDto>> ListarFinanceiroAsync(
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken
    )
    {
        (pagina, tamanhoPagina) = Paginacao.Normalizar(pagina, tamanhoPagina);
        var resultado = await financeiro.ListarAsync(pagina, tamanhoPagina, cancellationToken);
        return Paginacao.Mapear(resultado, Mapeamento.ParaDto);
    }
}

public sealed class DashboardService(
    IFinanceiroRepositorio financeiro,
    IProdutoRepositorio produtos,
    IRelogioNegocio relogio
) : IDashboardService
{
    public async Task<DashboardDto> ObterAsync(CancellationToken cancellationToken)
    {
        var (inicioDia, inicioMes) = relogio.ObterLimitesUtc();
        var totais = await financeiro.ObterTotaisAsync(inicioDia, inicioMes, cancellationToken);
        var estoqueBaixo = await produtos.ListarAsync(null, true, 1, 100, cancellationToken);
        var faturamentoLiquidoDia = totais.EntradasDia - totais.EstornosDia;
        var faturamentoLiquidoMes = totais.EntradasMes - totais.EstornosMes;
        return new(
            faturamentoLiquidoDia,
            faturamentoLiquidoMes,
            totais.SaidasDia,
            totais.SaidasMes,
            totais.EstornosDia,
            totais.EstornosMes,
            faturamentoLiquidoDia - totais.SaidasDia,
            faturamentoLiquidoMes - totais.SaidasMes,
            totais.VendasDia,
            totais.VendasMes,
            totais.VendasDia == 0 ? 0 : faturamentoLiquidoDia / totais.VendasDia,
            totais.VendasMes == 0 ? 0 : faturamentoLiquidoMes / totais.VendasMes,
            estoqueBaixo.Itens.Select(Mapeamento.ParaDto).ToList()
        );
    }
}
