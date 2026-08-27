namespace SistemaDeGestaoComercial.Aplicacao.Abstractions;

public interface ICepService
{
    Task<EnderecoCep?> ConsultarAsync(string cep, CancellationToken cancellationToken);
}

public sealed record EnderecoCep(
    string Cep,
    string Logradouro,
    string? Complemento,
    string Bairro,
    string Cidade,
    string Uf
);

public sealed record Pagina<T>(IReadOnlyList<T> Itens, int PaginaAtual, int TamanhoPagina, int TotalItens)
{
    public int TotalPaginas => (int)Math.Ceiling(TotalItens / (double)TamanhoPagina);
}
