using Microsoft.EntityFrameworkCore;
using SistemaDeGestaoComercial.Dominio.Entidades;

namespace SistemaDeGestaoComercial.Infraestrutura.Persistencia;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Venda> Vendas => Set<Venda>();
    public DbSet<ItemVenda> ItensVenda => Set<ItemVenda>();
    public DbSet<MovimentacaoEstoque> MovimentacoesEstoque => Set<MovimentacaoEstoque>();
    public DbSet<MovimentacaoFinanceira> MovimentacoesFinanceiras => Set<MovimentacaoFinanceira>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<RegistroIdempotencia> RegistrosIdempotencia => Set<RegistroIdempotencia>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
    public DbSet<AlertaEstoque> AlertasEstoque => Set<AlertaEstoque>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasSequence<long>("NumeroVendaSequence").StartsAt(1).IncrementsBy(1);
        modelBuilder.Entity<Cliente>(configuracaoEntidade =>
        {
            configuracaoEntidade.Property(cliente => cliente.Nome).HasMaxLength(LimitesDominio.Nome);
            configuracaoEntidade.Property(cliente => cliente.CPF).HasMaxLength(11).IsFixedLength();
            configuracaoEntidade.Property(cliente => cliente.Email).HasMaxLength(LimitesDominio.Email);
            configuracaoEntidade.Property(cliente => cliente.Telefone).HasMaxLength(LimitesDominio.Telefone);
            configuracaoEntidade.Property(cliente => cliente.CriadoPor).HasMaxLength(LimitesDominio.UsuarioAuditoria);
            configuracaoEntidade
                .Property(cliente => cliente.AtualizadoPor)
                .HasMaxLength(LimitesDominio.UsuarioAuditoria);
            configuracaoEntidade.HasIndex(propriedade => propriedade.CPF).IsUnique();
            configuracaoEntidade.HasIndex(propriedade => propriedade.Email).IsUnique().HasFilter("[Email] IS NOT NULL");
            configuracaoEntidade.OwnsOne(
                cliente => cliente.Endereco,
                endereco =>
                {
                    endereco.Property(valor => valor.CEP).HasMaxLength(LimitesDominio.Cep).IsFixedLength();
                    endereco.Property(valor => valor.Logradouro).HasMaxLength(LimitesDominio.Logradouro);
                    endereco.Property(valor => valor.Numero).HasMaxLength(LimitesDominio.NumeroEndereco);
                    endereco.Property(valor => valor.Complemento).HasMaxLength(LimitesDominio.ComplementoEndereco);
                    endereco.Property(valor => valor.Bairro).HasMaxLength(LimitesDominio.Bairro);
                    endereco.Property(valor => valor.Cidade).HasMaxLength(LimitesDominio.Cidade);
                    endereco.Property(valor => valor.UF).HasMaxLength(LimitesDominio.Uf).IsFixedLength();
                }
            );
        });
        modelBuilder.Entity<Produto>(configuracaoEntidade =>
        {
            configuracaoEntidade.Property(produto => produto.Codigo).HasMaxLength(LimitesDominio.CodigoProduto);
            configuracaoEntidade.Property(produto => produto.Nome).HasMaxLength(LimitesDominio.Nome);
            configuracaoEntidade.Property(produto => produto.Descricao).HasMaxLength(LimitesDominio.Descricao);
            configuracaoEntidade.Property(produto => produto.CriadoPor).HasMaxLength(LimitesDominio.UsuarioAuditoria);
            configuracaoEntidade
                .Property(produto => produto.AtualizadoPor)
                .HasMaxLength(LimitesDominio.UsuarioAuditoria);
            configuracaoEntidade.HasIndex(propriedade => propriedade.Codigo).IsUnique();
            configuracaoEntidade.Property(propriedade => propriedade.PrecoCusto).HasPrecision(18, 2);
            configuracaoEntidade.Property(propriedade => propriedade.PrecoVenda).HasPrecision(18, 2);
            configuracaoEntidade.Property(propriedade => propriedade.Versao).IsRowVersion();
        });
        modelBuilder.Entity<Venda>(configuracaoEntidade =>
        {
            configuracaoEntidade.Property(venda => venda.Numero).HasMaxLength(LimitesDominio.NumeroVenda);
            configuracaoEntidade.Property(venda => venda.CriadoPor).HasMaxLength(LimitesDominio.UsuarioAuditoria);
            configuracaoEntidade.HasIndex(propriedade => propriedade.Numero).IsUnique();
            configuracaoEntidade.HasIndex(venda => venda.DataVenda);
            configuracaoEntidade.HasIndex(venda => new { venda.Situacao, venda.DataVenda });
            configuracaoEntidade.Property(propriedade => propriedade.Subtotal).HasPrecision(18, 2);
            configuracaoEntidade.Property(propriedade => propriedade.Desconto).HasPrecision(18, 2);
            configuracaoEntidade.Property(propriedade => propriedade.Total).HasPrecision(18, 2);
            configuracaoEntidade
                .HasOne(propriedade => propriedade.Cliente)
                .WithMany(propriedade => propriedade.Vendas)
                .HasForeignKey(propriedade => propriedade.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ItemVenda>(configuracaoEntidade =>
        {
            configuracaoEntidade.Property(propriedade => propriedade.PrecoUnitario).HasPrecision(18, 2);
            configuracaoEntidade.Property(propriedade => propriedade.Desconto).HasPrecision(18, 2);
            configuracaoEntidade.Property(propriedade => propriedade.Total).HasPrecision(18, 2);
            configuracaoEntidade
                .HasOne(propriedade => propriedade.Produto)
                .WithMany(propriedade => propriedade.ItensVenda)
                .HasForeignKey(propriedade => propriedade.ProdutoId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder
            .Entity<MovimentacaoEstoque>()
            .HasOne(propriedade => propriedade.Produto)
            .WithMany()
            .HasForeignKey(propriedade => propriedade.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<MovimentacaoEstoque>(configuracaoEntidade =>
        {
            configuracaoEntidade.Property(movimento => movimento.Observacao).HasMaxLength(LimitesDominio.Observacao);
            configuracaoEntidade
                .Property(movimento => movimento.CriadoPor)
                .HasMaxLength(LimitesDominio.UsuarioAuditoria);
            configuracaoEntidade.HasIndex(movimento => movimento.CreatedAt);
            configuracaoEntidade.HasIndex(movimento => new { movimento.ProdutoId, movimento.CreatedAt });
        });
        modelBuilder.Entity<MovimentacaoFinanceira>(configuracaoEntidade =>
        {
            configuracaoEntidade.Property(movimento => movimento.Valor).HasPrecision(18, 2);
            configuracaoEntidade.Property(movimento => movimento.Descricao).HasMaxLength(LimitesDominio.Descricao);
            configuracaoEntidade
                .Property(movimento => movimento.CriadoPor)
                .HasMaxLength(LimitesDominio.UsuarioAuditoria);
            configuracaoEntidade.HasIndex(movimento => movimento.DataMovimentacao);
            configuracaoEntidade.HasIndex(movimento => new { movimento.TipoMovimentacao, movimento.DataMovimentacao });
        });
        modelBuilder.Entity<Usuario>(configuracaoEntidade =>
        {
            configuracaoEntidade.Property(usuario => usuario.Nome).HasMaxLength(LimitesDominio.Nome);
            configuracaoEntidade.Property(usuario => usuario.Email).HasMaxLength(LimitesDominio.Email);
            configuracaoEntidade.Property(usuario => usuario.SenhaHash).HasMaxLength(LimitesDominio.SenhaHash);
            configuracaoEntidade.Property(usuario => usuario.CriadoPor).HasMaxLength(LimitesDominio.UsuarioAuditoria);
            configuracaoEntidade
                .Property(usuario => usuario.AtualizadoPor)
                .HasMaxLength(LimitesDominio.UsuarioAuditoria);
            configuracaoEntidade.HasIndex(usuario => usuario.Email).IsUnique();
        });
        modelBuilder.Entity<RegistroIdempotencia>(configuracaoEntidade =>
        {
            configuracaoEntidade.ToTable("RegistrosIdempotencia");
            configuracaoEntidade.Property(registro => registro.Chave).HasMaxLength(100);
            configuracaoEntidade.Property(registro => registro.HashRequisicao).HasMaxLength(64).IsFixedLength();
            configuracaoEntidade.Property(registro => registro.CriadoPor).HasMaxLength(LimitesDominio.UsuarioAuditoria);
            configuracaoEntidade.HasIndex(registro => registro.Chave).IsUnique();
            configuracaoEntidade.HasIndex(registro => registro.CreatedAt);
            configuracaoEntidade
                .HasOne<Venda>()
                .WithMany()
                .HasForeignKey(registro => registro.VendaId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<OutboxMessage>(configuracao =>
        {
            configuracao.ToTable("OutboxMessages");
            configuracao.HasKey(x => x.Id);
            configuracao.Property(x => x.Tipo).HasMaxLength(200);
            configuracao.Property(x => x.Conteudo).HasColumnType("nvarchar(max)");
            configuracao.Property(x => x.Erro).HasMaxLength(1000);
            configuracao.Property(x => x.CorrelationId).HasMaxLength(100);
            configuracao.HasIndex(x => new { x.ProcessedAt, x.CreatedAt });
        });
        modelBuilder.Entity<InboxMessage>(configuracao =>
        {
            configuracao.ToTable("InboxMessages");
            configuracao.HasKey(x => new { x.MessageId, x.Consumer });
            configuracao.Property(x => x.Consumer).HasMaxLength(200);
        });
        modelBuilder.Entity<AlertaEstoque>(configuracao =>
        {
            configuracao.ToTable("AlertasEstoque");
            configuracao.Property(x => x.NumeroVenda).HasMaxLength(LimitesDominio.NumeroVenda);
            configuracao.Property(x => x.CriadoPor).HasMaxLength(LimitesDominio.UsuarioAuditoria);
            configuracao.HasIndex(x => x.ProdutoId);
            configuracao.HasIndex(x => new { x.VendaId, x.ProdutoId }).IsUnique();
            configuracao
                .HasOne(x => x.Produto)
                .WithMany()
                .HasForeignKey(x => x.ProdutoId)
                .OnDelete(DeleteBehavior.Restrict);
            configuracao
                .HasOne(x => x.Venda)
                .WithMany()
                .HasForeignKey(x => x.VendaId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
