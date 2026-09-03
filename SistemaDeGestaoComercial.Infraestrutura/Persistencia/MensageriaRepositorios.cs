using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SistemaDeGestaoComercial.Aplicacao.Abstractions;
using SistemaDeGestaoComercial.Aplicacao.Contratos;
using SistemaDeGestaoComercial.Dominio.Entidades;

namespace SistemaDeGestaoComercial.Infraestrutura.Persistencia;

internal sealed class OutboxRepositorio(AppDbContext contexto) : IOutboxRepositorio
{
    public void Adicionar(VendaRealizadaEvent evento, string? correlationId = null) =>
        contexto.OutboxMessages.Add(
            new OutboxMessage(
                evento.EventoId,
                nameof(VendaRealizadaEvent),
                JsonSerializer.Serialize(evento),
                DateTime.UtcNow,
                correlationId
            )
        );
}

internal sealed class InboxRepositorio(AppDbContext contexto) : IInboxRepositorio
{
    public Task<bool> JaProcessadaAsync(Guid messageId, string consumer, CancellationToken cancellationToken) =>
        contexto.InboxMessages.AnyAsync(x => x.MessageId == messageId && x.Consumer == consumer, cancellationToken);

    public void Adicionar(Guid messageId, string consumer) =>
        contexto.InboxMessages.Add(new InboxMessage(messageId, consumer));
}

internal sealed class AlertaEstoqueRepositorio(AppDbContext contexto) : IAlertaEstoqueRepositorio
{
    public Task<ResultadoPaginado<AlertaEstoque>> ListarAsync(int pagina, int tamanhoPagina, CancellationToken ct) =>
        ConsultaPaginada.ExecutarAsync(
            contexto
                .AlertasEstoque.AsNoTracking()
                .Include(x => x.Produto)
                .OrderBy(x => x.Visualizado)
                .ThenByDescending(x => x.CreatedAt),
            pagina,
            tamanhoPagina,
            ct
        );

    public Task<AlertaEstoque?> ObterAsync(Guid id, CancellationToken ct) =>
        contexto.AlertasEstoque.Include(x => x.Produto).SingleOrDefaultAsync(x => x.Id == id, ct);
}
