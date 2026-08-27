using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SistemaDeGestaoComercial.Aplicacao.Abstractions;
using SistemaDeGestaoComercial.Aplicacao.Contratos;
using SistemaDeGestaoComercial.Aplicacao.Servicos;
using SistemaDeGestaoComercial.Infraestrutura.Persistencia;
using SistemaDeGestaoComercial.Infraestrutura.Servicos;

namespace SistemaDeGestaoComercial.Infraestrutura;

public static class DependencyInjection
{
    public static IServiceCollection AddInfraestrutura(this IServiceCollection services, IConfiguration configuration)
    {
        var connection =
            configuration.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("ConnectionStrings:SqlServer não configurada.");
        var connectionBuilder = new SqlConnectionStringBuilder(connection);
        if (connectionBuilder.ConnectRetryCount < 3)
            connectionBuilder.ConnectRetryCount = 3;
        if (connectionBuilder.ConnectRetryInterval < 2)
            connectionBuilder.ConnectRetryInterval = 2;
        services.AddDbContext<AppDbContext>(opcoes => opcoes.UseSqlServer(connectionBuilder.ConnectionString));
        services.AddMemoryCache();
        services.AddHttpClient<ICepService, ViaCepService>(clienteHttp =>
            clienteHttp.BaseAddress = new Uri("https://viacep.com.br/ws/")
        );
        services.AddScoped<IUnidadeTrabalho, UnidadeTrabalho>();
        services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
        services.AddScoped<IClienteRepositorio, ClienteRepositorio>();
        services.AddScoped<IProdutoRepositorio, ProdutoRepositorio>();
        services.AddScoped<IEstoqueRepositorio, EstoqueRepositorio>();
        services.AddScoped<IVendaRepositorio, VendaRepositorio>();
        services.AddScoped<IFinanceiroRepositorio, FinanceiroRepositorio>();
        services.AddScoped<IAutenticacaoService, AutenticacaoService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IProdutoService, ProdutoService>();
        services.AddScoped<IEstoqueService, EstoqueService>();
        services.AddScoped<IVendaService, VendaService>();
        services.AddScoped<IFinanceiroService, FinanceiroService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IValidacaoSessaoService, ValidacaoSessaoService>();
        services.AddSingleton<ISenhaService, SenhaService>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<ICacheSessao, CacheSessao>();
        services.AddSingleton<IRelogioNegocio, RelogioNegocio>();
        return services;
    }
}
