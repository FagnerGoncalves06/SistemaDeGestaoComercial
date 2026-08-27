using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using SistemaDeGestaoComercial.Aplicacao.Abstractions;

namespace SistemaDeGestaoComercial.Infraestrutura.Servicos;

internal sealed class ViaCepService(HttpClient clienteHttp) : ICepService
{
    public async Task<EnderecoCep?> ConsultarAsync(string cep, CancellationToken cancellationToken)
    {
        var cepSemFormatacao = new string(cep.Where(char.IsDigit).ToArray());
        if (cepSemFormatacao.Length != 8)
            throw new ArgumentException("CEP deve possuir 8 dígitos.", nameof(cep));
        try
        {
            var resposta = await clienteHttp.GetAsync($"{cepSemFormatacao}/json/", cancellationToken);
            if (resposta.StatusCode == HttpStatusCode.NotFound)
                return null;
            resposta.EnsureSuccessStatusCode();
            var enderecoViaCep = await resposta.Content.ReadFromJsonAsync<ViaCepDto>(cancellationToken);
            return enderecoViaCep is null || enderecoViaCep.Erro
                ? null
                : new(
                    enderecoViaCep.Cep ?? cepSemFormatacao,
                    enderecoViaCep.Logradouro ?? "",
                    enderecoViaCep.Complemento,
                    enderecoViaCep.Bairro ?? "",
                    enderecoViaCep.Localidade ?? "",
                    enderecoViaCep.Uf ?? ""
                );
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    private sealed record ViaCepDto(
        [property: JsonPropertyName("cep")] string? Cep,
        [property: JsonPropertyName("logradouro")] string? Logradouro,
        [property: JsonPropertyName("complemento")] string? Complemento,
        [property: JsonPropertyName("bairro")] string? Bairro,
        [property: JsonPropertyName("localidade")] string? Localidade,
        [property: JsonPropertyName("uf")] string? Uf,
        [property: JsonPropertyName("erro")] bool Erro
    );
}
