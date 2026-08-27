using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using SistemaDeGestaoComercial.Aplicacao.Abstractions;
using SistemaDeGestaoComercial.Dominio.Entidades;

namespace SistemaDeGestaoComercial.Infraestrutura.Servicos;

internal sealed class RelogioNegocio(IConfiguration configuracao) : IRelogioNegocio
{
    private readonly TimeZoneInfo fusoHorario = TimeZoneInfo.FindSystemTimeZoneById(
        configuracao["Negocio:FusoHorario"] ?? "America/Sao_Paulo"
    );

    public DateTime UtcAgora => DateTime.UtcNow;

    public (DateTime InicioDiaUtc, DateTime InicioMesUtc) ObterLimitesUtc()
    {
        var agoraLocal = TimeZoneInfo.ConvertTimeFromUtc(UtcAgora, fusoHorario);
        var inicioDiaLocal = DateTime.SpecifyKind(agoraLocal.Date, DateTimeKind.Unspecified);
        var inicioMesLocal = new DateTime(agoraLocal.Year, agoraLocal.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);
        return (
            TimeZoneInfo.ConvertTimeToUtc(inicioDiaLocal, fusoHorario),
            TimeZoneInfo.ConvertTimeToUtc(inicioMesLocal, fusoHorario)
        );
    }
}

internal sealed class CacheSessao(IMemoryCache cache) : ICacheSessao
{
    private static readonly TimeSpan Duracao = TimeSpan.FromMinutes(1);
    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> invalidadores = new();

    public Task<bool?> ObterAsync(
        Guid usuarioId,
        int versaoToken,
        PerfilUsuario perfil,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(
            cache.TryGetValue<bool>(CriarChave(usuarioId, versaoToken, perfil), out var permitido)
                ? (bool?)permitido
                : null
        );
    }

    public Task ArmazenarAsync(
        Guid usuarioId,
        int versaoToken,
        PerfilUsuario perfil,
        bool permitido,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        var chave = CriarChave(usuarioId, versaoToken, perfil);
        var invalidador = invalidadores.GetOrAdd(usuarioId, _ => new CancellationTokenSource());
        cache.Set(
            chave,
            permitido,
            new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = Duracao }.AddExpirationToken(
                new CancellationChangeToken(invalidador.Token)
            )
        );
        return Task.CompletedTask;
    }

    public Task InvalidarAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (invalidadores.TryRemove(usuarioId, out var invalidador))
        {
            invalidador.Cancel();
            invalidador.Dispose();
        }
        return Task.CompletedTask;
    }

    private static string CriarChave(Guid usuarioId, int versaoToken, PerfilUsuario perfil) =>
        $"sessao:{usuarioId:N}:{versaoToken}:{perfil}";
}
