using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SistemaDeGestaoComercial.Aplicacao.Contratos;
using SistemaDeGestaoComercial.Dominio.Entidades;

namespace SistemaDeGestaoComercial.Infraestrutura.Persistencia;

public static class DadosIniciais
{
    public static async Task InicializarAsync(
        AppDbContext contexto,
        ISenhaService senhaService,
        IConfiguration configuracao,
        CancellationToken cancellationToken
    )
    {
        if (!configuracao.GetValue("Seed:Enabled", false))
            return;
        await contexto.Database.MigrateAsync(cancellationToken);
        if (await contexto.Usuarios.AnyAsync(cancellationToken))
            return;
        var adminSenha =
            configuracao["Seed:AdminPassword"]
            ?? throw new InvalidOperationException("Seed:AdminPassword não configurada.");
        var operadorSenha =
            configuracao["Seed:OperadorPassword"]
            ?? throw new InvalidOperationException("Seed:OperadorPassword não configurada.");
        var admin = new Usuario(
            "Administrador",
            "admin@gestao.test",
            senhaService.Hash(adminSenha),
            PerfilUsuario.Administrador,
            "seed"
        );
        var operador = new Usuario(
            "Operador",
            "operador@gestao.test",
            senhaService.Hash(operadorSenha),
            PerfilUsuario.Operador,
            "seed"
        );
        var cliente = new Cliente(
            "Cliente Demonstração",
            "52998224725",
            "cliente@exemplo.test",
            "11999999999",
            new DateOnly(1990, 1, 1),
            new Endereco("01001000", "Praça da Sé", "100", null, "Sé", "São Paulo", "SP"),
            "seed"
        );
        var produto = new Produto("PROD-001", "Produto Demonstração", "Item fictício", 10m, 20m, 5, "seed");
        var estoque = produto.Movimentar(TipoMovimentacaoEstoque.Entrada, 20, null, "Estoque inicial", "seed");
        var item = new ItemVenda(produto.Id, 2, produto.PrecoVenda, 0);
        var venda = new Venda("V-DEMO-001", cliente.Id, FormaPagamento.Pix, 0, [item], "seed");
        var baixa = produto.Movimentar(TipoMovimentacaoEstoque.Venda, 2, venda.Id, "Venda inicial", "seed");
        contexto.AddRange(
            admin,
            operador,
            cliente,
            produto,
            estoque,
            venda,
            baixa,
            new MovimentacaoFinanceira(
                TipoMovimentacaoFinanceira.Entrada,
                "Venda V-DEMO-001",
                venda.Total,
                venda.Id,
                "seed"
            ),
            new MovimentacaoFinanceira(TipoMovimentacaoFinanceira.Saida, "Material de escritório", 15m, null, "seed")
        );
        await contexto.SaveChangesAsync(cancellationToken);
    }
}
