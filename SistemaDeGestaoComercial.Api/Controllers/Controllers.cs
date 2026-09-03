using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SistemaDeGestaoComercial.Aplicacao.Abstractions;
using SistemaDeGestaoComercial.Aplicacao.Contratos;

namespace SistemaDeGestaoComercial.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Route("api/v1/auth")]
public sealed class AuthController(IAutenticacaoService autenticacaoService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<LoginDto>> Login(LoginEntrada entrada, CancellationToken cancellationToken)
    {
        var resultado = await autenticacaoService.LoginAsync(entrada, cancellationToken);
        Response.Cookies.Append(
            "gestao_access_token",
            resultado.Token,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Expires = new DateTimeOffset(resultado.ExpiraEm),
                Path = "/",
            }
        );
        return Ok(
            new
            {
                resultado.ExpiraEm,
                resultado.Nome,
                resultado.Perfil,
            }
        );
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("gestao_access_token", new CookieOptions { Path = "/" });
        return NoContent();
    }

    [HttpGet("session")]
    [Authorize]
    public IActionResult Session() =>
        Ok(new { Nome = User.FindFirstValue(ClaimTypes.Name), Perfil = User.FindFirstValue(ClaimTypes.Role) });
}

[ApiController]
[Route("api/usuarios")]
[Route("api/v1/usuarios")]
[Authorize(Roles = "Administrador")]
public sealed class UsuariosController(IUsuarioService usuarioService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Listar(
        int pagina = 1,
        int tamanhoPagina = 20,
        CancellationToken cancellationToken = default
    )
    {
        return Ok(await usuarioService.ListarUsuariosAsync(pagina, tamanhoPagina, cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> Criar(UsuarioEntrada entrada, CancellationToken cancellationToken)
    {
        var usuario = await usuarioService.CriarUsuarioAsync(entrada, UsuarioResponsavel, cancellationToken);
        return CreatedAtAction(nameof(Listar), new { usuarioId = usuario.Id }, usuario);
    }

    [HttpPut("{usuarioId:guid}")]
    public async Task<IActionResult> Atualizar(
        Guid usuarioId,
        UsuarioAtualizacao entrada,
        CancellationToken cancellationToken
    )
    {
        return Ok(
            await usuarioService.AtualizarUsuarioAsync(usuarioId, entrada, UsuarioResponsavel, cancellationToken)
        );
    }

    [HttpPut("{usuarioId:guid}/senha")]
    public async Task<IActionResult> TrocarSenha(
        Guid usuarioId,
        TrocaSenhaEntrada entrada,
        CancellationToken cancellationToken
    )
    {
        await usuarioService.TrocarSenhaAsync(usuarioId, entrada.NovaSenha, UsuarioResponsavel, cancellationToken);
        return NoContent();
    }
}

[ApiController]
[Route("api/clientes")]
[Route("api/v1/clientes")]
[Authorize]
public sealed class ClientesController(IClienteService clienteService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Listar(
        string? busca,
        int pagina = 1,
        int tamanhoPagina = 20,
        CancellationToken cancellationToken = default
    )
    {
        return Ok(await clienteService.ListarClientesAsync(busca, pagina, tamanhoPagina, cancellationToken));
    }

    [HttpGet("{clienteId:guid}")]
    public async Task<IActionResult> Obter(Guid clienteId, CancellationToken cancellationToken)
    {
        return Ok(await clienteService.ObterClienteAsync(clienteId, cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> Criar(ClienteEntrada entrada, CancellationToken cancellationToken)
    {
        var cliente = await clienteService.CriarClienteAsync(entrada, UsuarioResponsavel, cancellationToken);
        return CreatedAtAction(nameof(Obter), new { clienteId = cliente.Id }, cliente);
    }

    [HttpPut("{clienteId:guid}")]
    public async Task<IActionResult> Atualizar(
        Guid clienteId,
        ClienteAtualizacao entrada,
        CancellationToken cancellationToken
    )
    {
        return Ok(
            await clienteService.AtualizarClienteAsync(clienteId, entrada, UsuarioResponsavel, cancellationToken)
        );
    }

    [HttpDelete("{clienteId:guid}")]
    public async Task<IActionResult> Excluir(Guid clienteId, CancellationToken cancellationToken)
    {
        await clienteService.ExcluirClienteAsync(clienteId, UsuarioResponsavel, cancellationToken);
        return NoContent();
    }

    [HttpGet("{clienteId:guid}/compras")]
    public async Task<IActionResult> Historico(
        Guid clienteId,
        int pagina = 1,
        int tamanhoPagina = 20,
        CancellationToken cancellationToken = default
    )
    {
        return Ok(await clienteService.HistoricoClienteAsync(clienteId, pagina, tamanhoPagina, cancellationToken));
    }
}

[ApiController]
[Route("api/produtos")]
[Route("api/v1/produtos")]
[Authorize]
public sealed class ProdutosController(IProdutoService produtoService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Listar(
        string? busca,
        int pagina = 1,
        int tamanhoPagina = 20,
        bool estoqueBaixo = false,
        CancellationToken cancellationToken = default
    )
    {
        return Ok(
            await produtoService.ListarProdutosAsync(busca, pagina, tamanhoPagina, estoqueBaixo, cancellationToken)
        );
    }

    [HttpGet("{produtoId:guid}")]
    public async Task<IActionResult> Obter(Guid produtoId, CancellationToken cancellationToken)
    {
        return Ok(await produtoService.ObterProdutoAsync(produtoId, cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Criar(ProdutoEntrada entrada, CancellationToken cancellationToken)
    {
        var produto = await produtoService.CriarProdutoAsync(entrada, UsuarioResponsavel, cancellationToken);
        return CreatedAtAction(nameof(Obter), new { produtoId = produto.Id }, produto);
    }

    [HttpPut("{produtoId:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Atualizar(
        Guid produtoId,
        ProdutoAtualizacao entrada,
        CancellationToken cancellationToken
    )
    {
        return Ok(
            await produtoService.AtualizarProdutoAsync(produtoId, entrada, UsuarioResponsavel, cancellationToken)
        );
    }

    [HttpDelete("{produtoId:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Excluir(Guid produtoId, CancellationToken cancellationToken)
    {
        await produtoService.ExcluirProdutoAsync(produtoId, UsuarioResponsavel, cancellationToken);
        return NoContent();
    }
}

[ApiController]
[Route("api/estoque")]
[Route("api/v1/estoque")]
[Authorize(Roles = "Administrador")]
public sealed class EstoqueController(IEstoqueService estoqueService) : BaseController
{
    [HttpPost("movimentacoes")]
    public async Task<IActionResult> Movimentar(EstoqueEntrada entrada, CancellationToken cancellationToken)
    {
        return Ok(await estoqueService.MovimentarEstoqueAsync(entrada, UsuarioResponsavel, cancellationToken));
    }

    [HttpGet("movimentacoes")]
    public async Task<IActionResult> Listar(
        Guid? produtoId,
        int pagina = 1,
        int tamanhoPagina = 20,
        CancellationToken cancellationToken = default
    )
    {
        return Ok(await estoqueService.ListarMovimentosAsync(produtoId, pagina, tamanhoPagina, cancellationToken));
    }
}

[ApiController]
[Route("api/estoque/alertas")]
[Route("api/v1/estoque/alertas")]
[Authorize(Roles = "Administrador")]
public sealed class AlertasEstoqueController(IAlertaEstoqueService alertas) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Listar(
        int pagina = 1,
        int tamanhoPagina = 20,
        CancellationToken cancellationToken = default
    ) => Ok(await alertas.ListarAsync(pagina, tamanhoPagina, cancellationToken));

    [HttpPut("{id:guid}/visualizar")]
    public async Task<IActionResult> Visualizar(Guid id, CancellationToken cancellationToken)
    {
        await alertas.VisualizarAsync(id, UsuarioResponsavel, cancellationToken);
        return NoContent();
    }
}

[ApiController]
[Route("api/vendas")]
[Route("api/v1/vendas")]
[Authorize]
public sealed class VendasController(IVendaService vendaService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Listar(
        int pagina = 1,
        int tamanhoPagina = 20,
        CancellationToken cancellationToken = default
    )
    {
        return Ok(await vendaService.ListarVendasAsync(pagina, tamanhoPagina, cancellationToken));
    }

    [HttpGet("{vendaId:guid}")]
    public async Task<IActionResult> Obter(Guid vendaId, CancellationToken cancellationToken)
    {
        return Ok(await vendaService.ObterVendaAsync(vendaId, cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> Criar(VendaEntrada entrada, CancellationToken cancellationToken)
    {
        var chaveIdempotencia = Request.Headers["Idempotency-Key"].ToString();
        var venda = await vendaService.CriarVendaAsync(
            entrada,
            chaveIdempotencia,
            UsuarioResponsavel,
            cancellationToken
        );
        return CreatedAtAction(nameof(Obter), new { vendaId = venda.Id }, venda);
    }

    [HttpPost("{vendaId:guid}/cancelar")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Cancelar(Guid vendaId, CancellationToken cancellationToken)
    {
        return Ok(await vendaService.CancelarVendaAsync(vendaId, UsuarioResponsavel, cancellationToken));
    }

    [HttpGet("{vendaId:guid}/recibo")]
    public async Task<IActionResult> Recibo(Guid vendaId, CancellationToken cancellationToken)
    {
        return Ok(await vendaService.ObterReciboAsync(vendaId, cancellationToken));
    }
}

[ApiController]
[Route("api/financeiro")]
[Route("api/v1/financeiro")]
[Authorize(Roles = "Administrador")]
public sealed class FinanceiroController(IFinanceiroService financeiroService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Listar(
        int pagina = 1,
        int tamanhoPagina = 20,
        CancellationToken cancellationToken = default
    )
    {
        return Ok(await financeiroService.ListarFinanceiroAsync(pagina, tamanhoPagina, cancellationToken));
    }

    [HttpPost("despesas")]
    public async Task<IActionResult> Despesa(DespesaEntrada entrada, CancellationToken cancellationToken)
    {
        return Ok(await financeiroService.CriarDespesaAsync(entrada, UsuarioResponsavel, cancellationToken));
    }
}

[ApiController]
[Route("api/dashboard")]
[Route("api/v1/dashboard")]
[Authorize(Roles = "Administrador")]
public sealed class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Obter(CancellationToken cancellationToken)
    {
        return Ok(await dashboardService.ObterAsync(cancellationToken));
    }
}

[ApiController]
[Route("api/cep")]
[Route("api/v1/cep")]
[Authorize]
public sealed class CepController(ICepService gestaoService) : ControllerBase
{
    [HttpGet("{cep}")]
    public async Task<IActionResult> Consultar(string cep, CancellationToken cancellationToken)
    {
        var endereco = await gestaoService.ConsultarAsync(cep, cancellationToken);

        if (endereco is null)
        {
            return NotFound(
                new ProblemDetails { Title = "CEP não encontrado", Status = StatusCodes.Status404NotFound }
            );
        }

        return Ok(endereco);
    }
}

public abstract class BaseController : ControllerBase
{
    protected string UsuarioResponsavel => User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name ?? "sistema";
}
