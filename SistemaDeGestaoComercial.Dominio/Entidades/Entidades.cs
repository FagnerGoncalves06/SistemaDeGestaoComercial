using System.Net.Mail;

namespace SistemaDeGestaoComercial.Dominio.Entidades;

public abstract class EntidadeBase
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public string CriadoPor { get; protected set; } = string.Empty;
}

public static class LimitesDominio
{
    public const int Nome = 150;
    public const int Email = 254;
    public const int Telefone = 20;
    public const int UsuarioAuditoria = 254;
    public const int CodigoProduto = 50;
    public const int Descricao = 500;
    public const int Observacao = 500;
    public const int Cep = 8;
    public const int Logradouro = 200;
    public const int NumeroEndereco = 20;
    public const int ComplementoEndereco = 100;
    public const int Bairro = 100;
    public const int Cidade = 100;
    public const int Uf = 2;
    public const int NumeroVenda = 30;
    public const int SenhaHash = 500;
}

public abstract class EntidadeAuditavel : EntidadeBase
{
    public DateTime? UpdatedAt { get; protected set; }
    public string? AtualizadoPor { get; protected set; }

    protected void Auditar(string usuario)
    {
        UpdatedAt = DateTime.UtcNow;
        AtualizadoPor = Regras.Exigir(usuario, nameof(usuario), LimitesDominio.UsuarioAuditoria);
    }
}

public sealed class Cliente : EntidadeAuditavel
{
    private Cliente() { }

    public Cliente(
        string nome,
        string cpf,
        string? email,
        string telefone,
        DateOnly? dataNascimento,
        Endereco endereco,
        string usuario
    )
    {
        Nome = Regras.Exigir(nome, nameof(nome), LimitesDominio.Nome);
        CPF = Regras.Cpf(cpf);
        Email = Regras.Email(email);
        Telefone = Regras.Exigir(telefone, nameof(telefone), LimitesDominio.Telefone);
        if (dataNascimento > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ExcecaoDominio("Data de nascimento não pode estar no futuro.");
        DataNascimento = dataNascimento;
        Endereco = endereco;
        CriadoPor = Regras.Exigir(usuario, nameof(usuario), LimitesDominio.UsuarioAuditoria);
    }

    public string Nome { get; private set; } = string.Empty;
    public string CPF { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string Telefone { get; private set; } = string.Empty;
    public DateOnly? DataNascimento { get; private set; }
    public Endereco Endereco { get; private set; } = null!;
    public bool Ativo { get; private set; } = true;
    public ICollection<Venda> Vendas { get; private set; } = [];

    public void Atualizar(
        string nome,
        string? email,
        string telefone,
        DateOnly? nascimento,
        Endereco endereco,
        string usuario
    )
    {
        Nome = Regras.Exigir(nome, nameof(nome), LimitesDominio.Nome);
        Email = Regras.Email(email);
        Telefone = Regras.Exigir(telefone, nameof(telefone), LimitesDominio.Telefone);
        if (nascimento > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ExcecaoDominio("Data de nascimento não pode estar no futuro.");
        DataNascimento = nascimento;
        Endereco = endereco;
        Auditar(usuario);
    }

    public void Inativar(string usuario)
    {
        Ativo = false;
        Auditar(usuario);
    }
}

public sealed record Endereco
{
    private Endereco() { }

    public Endereco(
        string cep,
        string logradouro,
        string numero,
        string? complemento,
        string bairro,
        string cidade,
        string uf
    )
    {
        CEP = Regras.Cep(cep);
        Logradouro = Regras.Exigir(logradouro, nameof(logradouro), LimitesDominio.Logradouro);
        Numero = Regras.Exigir(numero, nameof(numero), LimitesDominio.NumeroEndereco);
        Complemento = Regras.Opcional(complemento, nameof(complemento), LimitesDominio.ComplementoEndereco);
        Bairro = Regras.Exigir(bairro, nameof(bairro), LimitesDominio.Bairro);
        Cidade = Regras.Exigir(cidade, nameof(cidade), LimitesDominio.Cidade);
        UF = Regras.Exigir(uf, nameof(uf), LimitesDominio.Uf).ToUpperInvariant();
        if (UF.Length != LimitesDominio.Uf)
            throw new ExcecaoDominio("UF deve possuir 2 caracteres.");
    }

    public string CEP { get; private init; } = string.Empty;
    public string Logradouro { get; private init; } = string.Empty;
    public string Numero { get; private init; } = string.Empty;
    public string? Complemento { get; private init; }
    public string Bairro { get; private init; } = string.Empty;
    public string Cidade { get; private init; } = string.Empty;
    public string UF { get; private init; } = string.Empty;
}

public sealed class Produto : EntidadeAuditavel
{
    private Produto() { }

    public Produto(
        string codigo,
        string nome,
        string? descricao,
        decimal precoCusto,
        decimal precoVenda,
        int estoqueMinimo,
        string usuario
    )
    {
        Codigo = Regras.Exigir(codigo, nameof(codigo), LimitesDominio.CodigoProduto);
        Nome = Regras.Exigir(nome, nameof(nome), LimitesDominio.Nome);
        ValidarValores(precoCusto, precoVenda, estoqueMinimo);
        Descricao = Regras.Opcional(descricao, nameof(descricao), LimitesDominio.Descricao);
        PrecoCusto = precoCusto;
        PrecoVenda = precoVenda;
        EstoqueMinimo = estoqueMinimo;
        CriadoPor = Regras.Exigir(usuario, nameof(usuario), LimitesDominio.UsuarioAuditoria);
    }

    public string Codigo { get; private set; } = string.Empty;
    public string Nome { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }
    public decimal PrecoCusto { get; private set; }
    public decimal PrecoVenda { get; private set; }
    public int QuantidadeEstoque { get; private set; }
    public int EstoqueMinimo { get; private set; }
    public bool Ativo { get; private set; } = true;
    public byte[] Versao { get; private set; } = null!;
    public ICollection<ItemVenda> ItensVenda { get; private set; } = [];

    public void Atualizar(
        string nome,
        string? descricao,
        decimal precoCusto,
        decimal precoVenda,
        int estoqueMinimo,
        string usuario
    )
    {
        ValidarValores(precoCusto, precoVenda, estoqueMinimo);
        Nome = Regras.Exigir(nome, nameof(nome), LimitesDominio.Nome);
        Descricao = Regras.Opcional(descricao, nameof(descricao), LimitesDominio.Descricao);
        PrecoCusto = precoCusto;
        PrecoVenda = precoVenda;
        EstoqueMinimo = estoqueMinimo;
        Auditar(usuario);
    }

    public void Inativar(string usuario)
    {
        Ativo = false;
        Auditar(usuario);
    }

    public MovimentacaoEstoque Movimentar(
        TipoMovimentacaoEstoque tipo,
        int quantidade,
        Guid? referencia,
        string? observacao,
        string usuario
    )
    {
        if (quantidade <= 0)
            throw new ExcecaoDominio("Quantidade deve ser maior que zero.");
        var anterior = QuantidadeEstoque;
        var soma = tipo is TipoMovimentacaoEstoque.Entrada or TipoMovimentacaoEstoque.Devolucao;
        var posterior = soma ? anterior + quantidade : anterior - quantidade;
        if (posterior < 0)
            throw new ExcecaoDominio($"Estoque insuficiente para {Nome}.");
        QuantidadeEstoque = posterior;
        Auditar(usuario);
        return new(Id, tipo, quantidade, anterior, posterior, referencia, observacao, usuario);
    }

    private static void ValidarValores(decimal precoCusto, decimal precoVenda, int estoqueMinimo)
    {
        if (precoCusto < 0 || precoVenda < 0)
            throw new ExcecaoDominio("Preços não podem ser negativos.");
        if (estoqueMinimo < 0)
            throw new ExcecaoDominio("Estoque mínimo não pode ser negativo.");
    }
}

public sealed class MovimentacaoEstoque : EntidadeBase
{
    private MovimentacaoEstoque() { }

    internal MovimentacaoEstoque(
        Guid produto,
        TipoMovimentacaoEstoque tipo,
        int quantidade,
        int anterior,
        int posterior,
        Guid? referencia,
        string? observacao,
        string usuario
    )
    {
        ProdutoId = produto;
        TipoMovimentacao = tipo;
        Quantidade = quantidade;
        QuantidadeAnterior = anterior;
        QuantidadePosterior = posterior;
        ReferenciaId = referencia;
        Observacao = Regras.Opcional(observacao, nameof(observacao), LimitesDominio.Observacao);
        CriadoPor = Regras.Exigir(usuario, nameof(usuario), LimitesDominio.UsuarioAuditoria);
    }

    public Guid ProdutoId { get; private set; }
    public Produto Produto { get; private set; } = null!;
    public TipoMovimentacaoEstoque TipoMovimentacao { get; private set; }
    public int Quantidade { get; private set; }
    public int QuantidadeAnterior { get; private set; }
    public int QuantidadePosterior { get; private set; }
    public Guid? ReferenciaId { get; private set; }
    public string? Observacao { get; private set; }
}

public sealed class AlertaEstoque : EntidadeBase
{
    private AlertaEstoque() { }

    public AlertaEstoque(Guid produtoId, Guid vendaId, string numeroVenda, int quantidadeAtual, int estoqueMinimo)
    {
        ProdutoId = produtoId;
        VendaId = vendaId;
        NumeroVenda = Regras.Exigir(numeroVenda, nameof(numeroVenda), LimitesDominio.NumeroVenda);
        QuantidadeAtual = quantidadeAtual;
        EstoqueMinimo = estoqueMinimo;
        CriadoPor = "mensageria";
    }

    public Guid ProdutoId { get; private set; }
    public Produto Produto { get; private set; } = null!;
    public Guid VendaId { get; private set; }
    public Venda Venda { get; private set; } = null!;
    public string NumeroVenda { get; private set; } = string.Empty;
    public int QuantidadeAtual { get; private set; }
    public int EstoqueMinimo { get; private set; }
    public bool Visualizado { get; private set; }
    public DateTime? VisualizadoAt { get; private set; }

    public void Visualizar()
    {
        if (Visualizado)
            return;
        Visualizado = true;
        VisualizadoAt = DateTime.UtcNow;
    }

    public static AlertaEstoque? CriarSeEstoqueBaixo(Produto produto, Guid vendaId, string numeroVenda) =>
        produto.QuantidadeEstoque <= produto.EstoqueMinimo
            ? new AlertaEstoque(produto.Id, vendaId, numeroVenda, produto.QuantidadeEstoque, produto.EstoqueMinimo)
            : null;
}

public sealed class Venda : EntidadeBase
{
    private Venda() { }

    public Venda(
        string numero,
        Guid? cliente,
        FormaPagamento pagamento,
        decimal desconto,
        IEnumerable<ItemVenda> itens,
        string usuario
    )
    {
        Numero = Regras.Exigir(numero, nameof(numero), LimitesDominio.NumeroVenda);
        ClienteId = cliente;
        DataVenda = DateTime.UtcNow;
        FormaPagamento = pagamento;
        CriadoPor = Regras.Exigir(usuario, nameof(usuario), LimitesDominio.UsuarioAuditoria);
        Itens = itens.ToList();
        if (Itens.Count == 0)
            throw new ExcecaoDominio("Venda deve possuir itens.");
        Subtotal = Itens.Sum(itemVenda => itemVenda.Total);
        if (desconto < 0 || desconto > Subtotal)
            throw new ExcecaoDominio("Desconto inválido.");
        Desconto = desconto;
        Total = Subtotal - desconto;
        Situacao = SituacaoVenda.Concluida;
    }

    public string Numero { get; private set; } = string.Empty;
    public Guid? ClienteId { get; private set; }
    public Cliente? Cliente { get; private set; }
    public DateTime DataVenda { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal Desconto { get; private set; }
    public decimal Total { get; private set; }
    public FormaPagamento FormaPagamento { get; private set; }
    public SituacaoVenda Situacao { get; private set; }
    public ICollection<ItemVenda> Itens { get; private set; } = [];

    public void Cancelar()
    {
        if (Situacao == SituacaoVenda.Cancelada)
            throw new ExcecaoDominio("Venda já está cancelada.");
        Situacao = SituacaoVenda.Cancelada;
    }
}

public sealed class ItemVenda
{
    private ItemVenda() { }

    public ItemVenda(Guid produto, int quantidade, decimal preco, decimal desconto)
    {
        if (quantidade <= 0 || preco < 0 || desconto < 0 || desconto > preco * quantidade)
            throw new ExcecaoDominio("Item de venda inválido.");
        ProdutoId = produto;
        Quantidade = quantidade;
        PrecoUnitario = preco;
        Desconto = desconto;
        Total = preco * quantidade - desconto;
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid VendaId { get; private set; }
    public Venda Venda { get; private set; } = null!;
    public Guid ProdutoId { get; private set; }
    public Produto Produto { get; private set; } = null!;
    public int Quantidade { get; private set; }
    public decimal PrecoUnitario { get; private set; }
    public decimal Desconto { get; private set; }
    public decimal Total { get; private set; }
}

public sealed class MovimentacaoFinanceira : EntidadeBase
{
    private MovimentacaoFinanceira() { }

    public MovimentacaoFinanceira(
        TipoMovimentacaoFinanceira tipo,
        string descricao,
        decimal valor,
        Guid? venda,
        string usuario
    )
    {
        if (valor <= 0)
            throw new ExcecaoDominio("Valor deve ser maior que zero.");
        TipoMovimentacao = tipo;
        Descricao = Regras.Exigir(descricao, nameof(descricao), LimitesDominio.Descricao);
        Valor = valor;
        VendaId = venda;
        DataMovimentacao = DateTime.UtcNow;
        CriadoPor = Regras.Exigir(usuario, nameof(usuario), LimitesDominio.UsuarioAuditoria);
    }

    public TipoMovimentacaoFinanceira TipoMovimentacao { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public decimal Valor { get; private set; }
    public DateTime DataMovimentacao { get; private set; }
    public Guid? VendaId { get; private set; }
    public Venda? Venda { get; private set; }
}

public sealed class Usuario : EntidadeAuditavel
{
    private Usuario() { }

    public Usuario(string nome, string email, string senhaHash, PerfilUsuario perfil, string criadoPor)
    {
        Nome = Regras.Exigir(nome, nameof(nome), LimitesDominio.Nome);
        Email = Regras.Email(email) ?? throw new ExcecaoDominio("Email obrigatório.");
        SenhaHash = Regras.Exigir(senhaHash, nameof(senhaHash), LimitesDominio.SenhaHash);
        Perfil = perfil;
        CriadoPor = Regras.Exigir(criadoPor, nameof(criadoPor), LimitesDominio.UsuarioAuditoria);
    }

    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string SenhaHash { get; private set; } = string.Empty;
    public PerfilUsuario Perfil { get; private set; }
    public bool Ativo { get; private set; } = true;
    public int VersaoToken { get; private set; } = 1;

    public void Atualizar(string nome, PerfilUsuario perfil, bool ativo, string usuario)
    {
        Nome = Regras.Exigir(nome, nameof(nome), LimitesDominio.Nome);
        if (Perfil != perfil || Ativo != ativo)
            VersaoToken++;
        Perfil = perfil;
        Ativo = ativo;
        Auditar(usuario);
    }

    public void TrocarSenha(string hash, string usuario)
    {
        SenhaHash = Regras.Exigir(hash, nameof(hash), LimitesDominio.SenhaHash);
        VersaoToken++;
        Auditar(usuario);
    }
}

public enum TipoMovimentacaoEstoque
{
    Entrada,
    Venda,
    Ajuste,
    Devolucao,
}

public enum TipoMovimentacaoFinanceira
{
    Entrada,
    Saida,
    Estorno,
}

public enum FormaPagamento
{
    Dinheiro,
    Pix,
    CartaoDebito,
    CartaoCredito,
}

public enum SituacaoVenda
{
    Concluida,
    Cancelada,
}

public enum PerfilUsuario
{
    Administrador,
    Operador,
}

public sealed class ExcecaoDominio(string mensagem) : Exception(mensagem);

public sealed class EntidadeNaoEncontradaException(string mensagem) : Exception(mensagem);

internal static class Regras
{
    public static string Exigir(string valor, string campo, int tamanhoMaximo = int.MaxValue)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ExcecaoDominio($"{campo} é obrigatório.");
        var valorNormalizado = valor.Trim();
        return valorNormalizado.Length <= tamanhoMaximo
            ? valorNormalizado
            : throw new ExcecaoDominio($"{campo} deve possuir no máximo {tamanhoMaximo} caracteres.");
    }

    public static string? Opcional(string? valor, string campo, int tamanhoMaximo)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return null;
        var valorNormalizado = valor.Trim();
        return valorNormalizado.Length <= tamanhoMaximo
            ? valorNormalizado
            : throw new ExcecaoDominio($"{campo} deve possuir no máximo {tamanhoMaximo} caracteres.");
    }

    public static string Cep(string valor)
    {
        var cep = Digitos(valor);
        return cep.Length == 8 ? cep : throw new ExcecaoDominio("CEP deve possuir 8 dígitos.");
    }

    public static string? Email(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return null;
        var emailNormalizado = valor.Trim().ToLowerInvariant();
        if (emailNormalizado.Length > LimitesDominio.Email)
            throw new ExcecaoDominio($"Email deve possuir no máximo {LimitesDominio.Email} caracteres.");
        return MailAddress.TryCreate(emailNormalizado, out _)
            ? emailNormalizado
            : throw new ExcecaoDominio("Email inválido.");
    }

    public static string Cpf(string valor)
    {
        var cpf = Digitos(valor);
        if (cpf.Length != 11 || cpf.Distinct().Count() == 1)
            throw new ExcecaoDominio("CPF inválido.");
        var digitosCpf = cpf.Select(caractere => caractere - '0').ToArray();
        for (var indiceDigitoVerificador = 9; indiceDigitoVerificador < 11; indiceDigitoVerificador++)
        {
            var somaPonderada = 0;
            for (var indiceDigito = 0; indiceDigito < indiceDigitoVerificador; indiceDigito++)
                somaPonderada += digitosCpf[indiceDigito] * (indiceDigitoVerificador + 1 - indiceDigito);
            var restoDivisao = somaPonderada % 11;
            if (digitosCpf[indiceDigitoVerificador] != (restoDivisao < 2 ? 0 : 11 - restoDivisao))
                throw new ExcecaoDominio("CPF inválido.");
        }
        return cpf;
    }

    private static string Digitos(string valor) => new(valor.Where(char.IsDigit).ToArray());
}
