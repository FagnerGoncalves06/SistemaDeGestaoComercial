namespace SistemaDeGestaoComercial.Infraestrutura.Persistencia;

public sealed class RegistroIdempotencia
{
    private RegistroIdempotencia() { }

    public RegistroIdempotencia(string chave, string hashRequisicao, Guid vendaId, string criadoPor)
    {
        Id = Guid.NewGuid();
        Chave = chave;
        HashRequisicao = hashRequisicao;
        VendaId = vendaId;
        CriadoPor = criadoPor;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Chave { get; private set; } = string.Empty;
    public string HashRequisicao { get; private set; } = string.Empty;
    public Guid VendaId { get; private set; }
    public string CriadoPor { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
}
