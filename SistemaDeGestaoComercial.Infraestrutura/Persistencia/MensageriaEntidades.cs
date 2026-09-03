namespace SistemaDeGestaoComercial.Infraestrutura.Persistencia;

public sealed class OutboxMessage
{
    private OutboxMessage() { }

    public OutboxMessage(Guid id, string tipo, string conteudo, DateTime createdAt, string? correlationId)
    {
        Id = id;
        Tipo = tipo;
        Conteudo = conteudo;
        CreatedAt = createdAt;
        CorrelationId = correlationId;
    }

    public Guid Id { get; private set; }
    public string Tipo { get; private set; } = string.Empty;
    public string Conteudo { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public int Tentativas { get; private set; }
    public string? Erro { get; private set; }
    public string? CorrelationId { get; private set; }

    public void MarcarProcessada()
    {
        ProcessedAt = DateTime.UtcNow;
        Erro = null;
    }

    public void RegistrarFalha(string erro)
    {
        Tentativas++;
        Erro = erro.Length <= 1000 ? erro : erro[..1000];
    }
}

public sealed class InboxMessage
{
    private InboxMessage() { }

    public InboxMessage(Guid messageId, string consumer)
    {
        MessageId = messageId;
        Consumer = consumer;
        ProcessedAt = DateTime.UtcNow;
    }

    public Guid MessageId { get; private set; }
    public string Consumer { get; private set; } = string.Empty;
    public DateTime ProcessedAt { get; private set; }
}
