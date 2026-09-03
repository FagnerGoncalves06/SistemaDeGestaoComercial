using SistemaDeGestaoComercial.Dominio.Entidades;

namespace SistemaDeGestaoComercial.Testes.Unitarios;

public sealed class RegrasDominioTests
{
    [Fact]
    public void AlertaEstoque_SoEhCriadoQuandoQuantidadeAtingeMinimo()
    {
        var produto = new Produto("P1", "Produto", null, 1, 2, 5, "teste");
        produto.Movimentar(TipoMovimentacaoEstoque.Entrada, 6, null, null, "teste");
        Assert.Null(AlertaEstoque.CriarSeEstoqueBaixo(produto, Guid.NewGuid(), "V000000000001"));
        produto.Movimentar(TipoMovimentacaoEstoque.Venda, 1, null, null, "teste");
        Assert.NotNull(AlertaEstoque.CriarSeEstoqueBaixo(produto, Guid.NewGuid(), "V000000000002"));
    }

    [Fact]
    public void Cliente_RejeitaCpfInvalido() =>
        Assert.Throws<ExcecaoDominio>(() =>
            new Cliente(
                "Cliente",
                "11111111111",
                null,
                "",
                null,
                new Endereco("01001000", "Praça", "1", null, "Sé", "São Paulo", "SP"),
                "teste"
            )
        );

    [Fact]
    public void Cliente_AceitaCpfValido()
    {
        var cliente = new Cliente(
            "Cliente",
            "529.982.247-25",
            "cliente@exemplo.test",
            "11999999999",
            null,
            new Endereco("01001000", "Praça", "1", null, "Sé", "São Paulo", "SP"),
            "teste"
        );
        Assert.Equal("52998224725", cliente.CPF);
    }

    [Fact]
    public void Produto_RejeitaPrecoNegativo() =>
        Assert.Throws<ExcecaoDominio>(() => new Produto("P1", "Produto", null, -1m, 1m, 0, "teste"));

    [Fact]
    public void Estoque_EntradaRegistraSaldos()
    {
        var produto = new Produto("P1", "Produto", null, 1m, 2m, 2, "teste");
        var movimento = produto.Movimentar(TipoMovimentacaoEstoque.Entrada, 5, null, "Inicial", "teste");
        Assert.Equal(5, produto.QuantidadeEstoque);
        Assert.Equal(0, movimento.QuantidadeAnterior);
        Assert.Equal(5, movimento.QuantidadePosterior);
    }

    [Fact]
    public void Estoque_NuncaFicaNegativo()
    {
        var produto = new Produto("P1", "Produto", null, 1m, 2m, 2, "teste");
        Assert.Throws<ExcecaoDominio>(() => produto.Movimentar(TipoMovimentacaoEstoque.Venda, 1, null, null, "teste"));
    }

    [Fact]
    public void Venda_CalculaTotaisEPreservaPreco()
    {
        var item = new ItemVenda(Guid.NewGuid(), 2, 15m, 2m);
        var venda = new Venda("V1", null, FormaPagamento.Pix, 3m, [item], "teste");
        Assert.Equal(28m, venda.Subtotal);
        Assert.Equal(25m, venda.Total);
        Assert.Equal(15m, item.PrecoUnitario);
    }

    [Fact]
    public void Venda_CancelamentoDuplicadoFalha()
    {
        var venda = new Venda(
            "V1",
            null,
            FormaPagamento.Dinheiro,
            0,
            [new ItemVenda(Guid.NewGuid(), 1, 10m, 0)],
            "teste"
        );
        venda.Cancelar();
        Assert.Throws<ExcecaoDominio>(venda.Cancelar);
    }

    [Fact]
    public void Financeiro_RejeitaValorNaoPositivo() =>
        Assert.Throws<ExcecaoDominio>(() =>
            new MovimentacaoFinanceira(TipoMovimentacaoFinanceira.Saida, "Despesa", 0, null, "teste")
        );

    [Fact]
    public void Cliente_RejeitaDataNascimentoFutura() =>
        Assert.Throws<ExcecaoDominio>(() =>
            new Cliente(
                "Cliente",
                "52998224725",
                null,
                "11999999999",
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                new Endereco("01001000", "Praça", "1", null, "Sé", "São Paulo", "SP"),
                "teste"
            )
        );

    [Fact]
    public void Endereco_RejeitaUfInvalida() =>
        Assert.Throws<ExcecaoDominio>(() => new Endereco("01001000", "Praça", "1", null, "Sé", "São Paulo", "S"));

    [Fact]
    public void Usuario_AlteracaoDePerfilRevogaTokensAnteriores()
    {
        var usuario = new Usuario("Usuário", "usuario@teste.local", "hash-valido", PerfilUsuario.Operador, "teste");
        var versaoAnterior = usuario.VersaoToken;
        usuario.Atualizar("Usuário", PerfilUsuario.Administrador, true, "teste");
        Assert.True(usuario.VersaoToken > versaoAnterior);
    }
}
