namespace SistemaDeGestaoComercial.Aplicacao.Contratos;

public sealed record VendaRealizadaEvent(
    Guid EventoId,
    Guid VendaId,
    string NumeroVenda,
    Guid? ClienteId,
    decimal Total,
    DateTime DataVenda,
    IReadOnlyCollection<ItemVendaRealizadaEvent> Itens
);

public sealed record ItemVendaRealizadaEvent(Guid ProdutoId, int Quantidade);

public sealed record AlertaEstoqueDto(
    Guid Id,
    Guid ProdutoId,
    string Produto,
    Guid VendaId,
    string NumeroVenda,
    int QuantidadeAtual,
    int EstoqueMinimo,
    DateTime CreatedAt,
    bool Visualizado
);
