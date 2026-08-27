IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821201520_Inicial'
)
BEGIN
    CREATE TABLE [Clientes] (
        [Id] uniqueidentifier NOT NULL,
        [Nome] nvarchar(max) NOT NULL,
        [CPF] nvarchar(450) NOT NULL,
        [Email] nvarchar(450) NULL,
        [Telefone] nvarchar(max) NOT NULL,
        [DataNascimento] date NULL,
        [Endereco_CEP] nvarchar(max) NOT NULL,
        [Endereco_Logradouro] nvarchar(max) NOT NULL,
        [Endereco_Numero] nvarchar(max) NOT NULL,
        [Endereco_Complemento] nvarchar(max) NULL,
        [Endereco_Bairro] nvarchar(max) NOT NULL,
        [Endereco_Cidade] nvarchar(max) NOT NULL,
        [Endereco_UF] nvarchar(max) NOT NULL,
        [Ativo] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CriadoPor] nvarchar(max) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [AtualizadoPor] nvarchar(max) NULL,
        CONSTRAINT [PK_Clientes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821201520_Inicial'
)
BEGIN
    CREATE TABLE [Produtos] (
        [Id] uniqueidentifier NOT NULL,
        [Codigo] nvarchar(450) NOT NULL,
        [Nome] nvarchar(max) NOT NULL,
        [Descricao] nvarchar(max) NULL,
        [PrecoCusto] decimal(18,2) NOT NULL,
        [PrecoVenda] decimal(18,2) NOT NULL,
        [QuantidadeEstoque] int NOT NULL,
        [EstoqueMinimo] int NOT NULL,
        [Ativo] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CriadoPor] nvarchar(max) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [AtualizadoPor] nvarchar(max) NULL,
        CONSTRAINT [PK_Produtos] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821201520_Inicial'
)
BEGIN
    CREATE TABLE [Usuarios] (
        [Id] uniqueidentifier NOT NULL,
        [Nome] nvarchar(max) NOT NULL,
        [Email] nvarchar(450) NOT NULL,
        [SenhaHash] nvarchar(max) NOT NULL,
        [Perfil] int NOT NULL,
        [Ativo] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CriadoPor] nvarchar(max) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [AtualizadoPor] nvarchar(max) NULL,
        CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821201520_Inicial'
)
BEGIN
    CREATE TABLE [Vendas] (
        [Id] uniqueidentifier NOT NULL,
        [Numero] nvarchar(450) NOT NULL,
        [ClienteId] uniqueidentifier NULL,
        [DataVenda] datetime2 NOT NULL,
        [Subtotal] decimal(18,2) NOT NULL,
        [Desconto] decimal(18,2) NOT NULL,
        [Total] decimal(18,2) NOT NULL,
        [FormaPagamento] int NOT NULL,
        [Situacao] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CriadoPor] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Vendas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Vendas_Clientes_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clientes] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821201520_Inicial'
)
BEGIN
    CREATE TABLE [MovimentacoesEstoque] (
        [Id] uniqueidentifier NOT NULL,
        [ProdutoId] uniqueidentifier NOT NULL,
        [TipoMovimentacao] int NOT NULL,
        [Quantidade] int NOT NULL,
        [QuantidadeAnterior] int NOT NULL,
        [QuantidadePosterior] int NOT NULL,
        [ReferenciaId] uniqueidentifier NULL,
        [Observacao] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CriadoPor] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_MovimentacoesEstoque] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MovimentacoesEstoque_Produtos_ProdutoId] FOREIGN KEY ([ProdutoId]) REFERENCES [Produtos] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821201520_Inicial'
)
BEGIN
    CREATE TABLE [ItensVenda] (
        [Id] uniqueidentifier NOT NULL,
        [VendaId] uniqueidentifier NOT NULL,
        [ProdutoId] uniqueidentifier NOT NULL,
        [Quantidade] int NOT NULL,
        [PrecoUnitario] decimal(18,2) NOT NULL,
        [Desconto] decimal(18,2) NOT NULL,
        [Total] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_ItensVenda] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ItensVenda_Produtos_ProdutoId] FOREIGN KEY ([ProdutoId]) REFERENCES [Produtos] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ItensVenda_Vendas_VendaId] FOREIGN KEY ([VendaId]) REFERENCES [Vendas] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821201520_Inicial'
)
BEGIN
    CREATE TABLE [MovimentacoesFinanceiras] (
        [Id] uniqueidentifier NOT NULL,
        [TipoMovimentacao] int NOT NULL,
        [Descricao] nvarchar(max) NOT NULL,
        [Valor] decimal(18,2) NOT NULL,
        [DataMovimentacao] datetime2 NOT NULL,
        [VendaId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CriadoPor] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_MovimentacoesFinanceiras] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MovimentacoesFinanceiras_Vendas_VendaId] FOREIGN KEY ([VendaId]) REFERENCES [Vendas] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821201520_Inicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Clientes_CPF] ON [Clientes] ([CPF]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821201520_Inicial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Clientes_Email] ON [Clientes] ([Email]) WHERE [Email] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821201520_Inicial'
)
BEGIN
    CREATE INDEX [IX_ItensVenda_ProdutoId] ON [ItensVenda] ([ProdutoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821201520_Inicial'
)
BEGIN
    CREATE INDEX [IX_ItensVenda_VendaId] ON [ItensVenda] ([VendaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821201520_Inicial'
)
BEGIN
    CREATE INDEX [IX_MovimentacoesEstoque_ProdutoId] ON [MovimentacoesEstoque] ([ProdutoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821201520_Inicial'
)
BEGIN
    CREATE INDEX [IX_MovimentacoesFinanceiras_VendaId] ON [MovimentacoesFinanceiras] ([VendaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821201520_Inicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Produtos_Codigo] ON [Produtos] ([Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821201520_Inicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Usuarios_Email] ON [Usuarios] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821201520_Inicial'
)
BEGIN
    CREATE INDEX [IX_Vendas_ClienteId] ON [Vendas] ([ClienteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821201520_Inicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Vendas_Numero] ON [Vendas] ([Numero]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821201520_Inicial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260821201520_Inicial', N'10.0.0');
END;

COMMIT;
GO
BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    CREATE SEQUENCE [NumeroVendaSequence] START WITH 1 INCREMENT BY 1 NO CYCLE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    DROP INDEX [IX_Vendas_Numero] ON [Vendas];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    DROP INDEX [IX_Usuarios_Email] ON [Usuarios];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    DROP INDEX [IX_Produtos_Codigo] ON [Produtos];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    DROP INDEX [IX_Clientes_CPF] ON [Clientes];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    DROP INDEX [IX_Clientes_Email] ON [Clientes];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Vendas]') AND [c].[name] = N'Numero');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Vendas] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [Vendas] ALTER COLUMN [Numero] nvarchar(30) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    DECLARE @var1 nvarchar(max);
    SELECT @var1 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Vendas]') AND [c].[name] = N'CriadoPor');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Vendas] DROP CONSTRAINT ' + @var1 + ';');
    ALTER TABLE [Vendas] ALTER COLUMN [CriadoPor] nvarchar(254) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    DECLARE @var2 nvarchar(max);
    SELECT @var2 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Usuarios]') AND [c].[name] = N'SenhaHash');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Usuarios] DROP CONSTRAINT ' + @var2 + ';');
    ALTER TABLE [Usuarios] ALTER COLUMN [SenhaHash] nvarchar(500) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    DECLARE @var3 nvarchar(max);
    SELECT @var3 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Usuarios]') AND [c].[name] = N'Nome');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Usuarios] DROP CONSTRAINT ' + @var3 + ';');
    ALTER TABLE [Usuarios] ALTER COLUMN [Nome] nvarchar(150) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    DECLARE @var4 nvarchar(max);
    SELECT @var4 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Usuarios]') AND [c].[name] = N'Email');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Usuarios] DROP CONSTRAINT ' + @var4 + ';');
    ALTER TABLE [Usuarios] ALTER COLUMN [Email] nvarchar(254) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    DECLARE @var5 nvarchar(max);
    SELECT @var5 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Usuarios]') AND [c].[name] = N'CriadoPor');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Usuarios] DROP CONSTRAINT ' + @var5 + ';');
    ALTER TABLE [Usuarios] ALTER COLUMN [CriadoPor] nvarchar(254) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    DECLARE @var6 nvarchar(max);
    SELECT @var6 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Usuarios]') AND [c].[name] = N'AtualizadoPor');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Usuarios] DROP CONSTRAINT ' + @var6 + ';');
    ALTER TABLE [Usuarios] ALTER COLUMN [AtualizadoPor] nvarchar(254) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    ALTER TABLE [Usuarios] ADD [VersaoToken] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    DECLARE @var7 nvarchar(max);
    SELECT @var7 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Produtos]') AND [c].[name] = N'Nome');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Produtos] DROP CONSTRAINT ' + @var7 + ';');
    ALTER TABLE [Produtos] ALTER COLUMN [Nome] nvarchar(150) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    DECLARE @var8 nvarchar(max);
    SELECT @var8 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Produtos]') AND [c].[name] = N'Descricao');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Produtos] DROP CONSTRAINT ' + @var8 + ';');
    ALTER TABLE [Produtos] ALTER COLUMN [Descricao] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    DECLARE @var9 nvarchar(max);
    SELECT @var9 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Produtos]') AND [c].[name] = N'CriadoPor');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [Produtos] DROP CONSTRAINT ' + @var9 + ';');
    ALTER TABLE [Produtos] ALTER COLUMN [CriadoPor] nvarchar(254) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    DECLARE @var10 nvarchar(max);
    SELECT @var10 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Produtos]') AND [c].[name] = N'Codigo');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [Produtos] DROP CONSTRAINT ' + @var10 + ';');
    ALTER TABLE [Produtos] ALTER COLUMN [Codigo] nvarchar(50) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    DECLARE @var11 nvarchar(max);
    SELECT @var11 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Produtos]') AND [c].[name] = N'AtualizadoPor');
    IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [Produtos] DROP CONSTRAINT ' + @var11 + ';');
    ALTER TABLE [Produtos] ALTER COLUMN [AtualizadoPor] nvarchar(254) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    ALTER TABLE [Produtos] ADD [Versao] rowversion NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    DECLARE @var12 nvarchar(max);
    SELECT @var12 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MovimentacoesFinanceiras]') AND [c].[name] = N'Descricao');
    IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [MovimentacoesFinanceiras] DROP CONSTRAINT ' + @var12 + ';');
    ALTER TABLE [MovimentacoesFinanceiras] ALTER COLUMN [Descricao] nvarchar(500) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    DECLARE @var13 nvarchar(max);
    SELECT @var13 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MovimentacoesFinanceiras]') AND [c].[name] = N'CriadoPor');
    IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [MovimentacoesFinanceiras] DROP CONSTRAINT ' + @var13 + ';');
    ALTER TABLE [MovimentacoesFinanceiras] ALTER COLUMN [CriadoPor] nvarchar(254) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    DECLARE @var14 nvarchar(max);
    SELECT @var14 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MovimentacoesEstoque]') AND [c].[name] = N'Observacao');
    IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [MovimentacoesEstoque] DROP CONSTRAINT ' + @var14 + ';');
    ALTER TABLE [MovimentacoesEstoque] ALTER COLUMN [Observacao] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    DECLARE @var15 nvarchar(max);
    SELECT @var15 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MovimentacoesEstoque]') AND [c].[name] = N'CriadoPor');
    IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [MovimentacoesEstoque] DROP CONSTRAINT ' + @var15 + ';');
    ALTER TABLE [MovimentacoesEstoque] ALTER COLUMN [CriadoPor] nvarchar(254) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    DECLARE @var16 nvarchar(max);
    SELECT @var16 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clientes]') AND [c].[name] = N'Telefone');
    IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [Clientes] DROP CONSTRAINT ' + @var16 + ';');
    ALTER TABLE [Clientes] ALTER COLUMN [Telefone] nvarchar(20) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    DECLARE @var17 nvarchar(max);
    SELECT @var17 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clientes]') AND [c].[name] = N'Nome');
    IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [Clientes] DROP CONSTRAINT ' + @var17 + ';');
    ALTER TABLE [Clientes] ALTER COLUMN [Nome] nvarchar(150) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    DECLARE @var18 nvarchar(max);
    SELECT @var18 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clientes]') AND [c].[name] = N'Endereco_UF');
    IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [Clientes] DROP CONSTRAINT ' + @var18 + ';');
    ALTER TABLE [Clientes] ALTER COLUMN [Endereco_UF] nchar(2) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    DECLARE @var19 nvarchar(max);
    SELECT @var19 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clientes]') AND [c].[name] = N'Endereco_Numero');
    IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [Clientes] DROP CONSTRAINT ' + @var19 + ';');
    ALTER TABLE [Clientes] ALTER COLUMN [Endereco_Numero] nvarchar(20) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    DECLARE @var20 nvarchar(max);
    SELECT @var20 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clientes]') AND [c].[name] = N'Endereco_Logradouro');
    IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [Clientes] DROP CONSTRAINT ' + @var20 + ';');
    ALTER TABLE [Clientes] ALTER COLUMN [Endereco_Logradouro] nvarchar(200) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    DECLARE @var21 nvarchar(max);
    SELECT @var21 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clientes]') AND [c].[name] = N'Endereco_Complemento');
    IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [Clientes] DROP CONSTRAINT ' + @var21 + ';');
    ALTER TABLE [Clientes] ALTER COLUMN [Endereco_Complemento] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    DECLARE @var22 nvarchar(max);
    SELECT @var22 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clientes]') AND [c].[name] = N'Endereco_Cidade');
    IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [Clientes] DROP CONSTRAINT ' + @var22 + ';');
    ALTER TABLE [Clientes] ALTER COLUMN [Endereco_Cidade] nvarchar(100) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    DECLARE @var23 nvarchar(max);
    SELECT @var23 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clientes]') AND [c].[name] = N'Endereco_CEP');
    IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [Clientes] DROP CONSTRAINT ' + @var23 + ';');
    ALTER TABLE [Clientes] ALTER COLUMN [Endereco_CEP] nchar(8) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    DECLARE @var24 nvarchar(max);
    SELECT @var24 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clientes]') AND [c].[name] = N'Endereco_Bairro');
    IF @var24 IS NOT NULL EXEC(N'ALTER TABLE [Clientes] DROP CONSTRAINT ' + @var24 + ';');
    ALTER TABLE [Clientes] ALTER COLUMN [Endereco_Bairro] nvarchar(100) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    DECLARE @var25 nvarchar(max);
    SELECT @var25 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clientes]') AND [c].[name] = N'Email');
    IF @var25 IS NOT NULL EXEC(N'ALTER TABLE [Clientes] DROP CONSTRAINT ' + @var25 + ';');
    ALTER TABLE [Clientes] ALTER COLUMN [Email] nvarchar(254) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    DECLARE @var26 nvarchar(max);
    SELECT @var26 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clientes]') AND [c].[name] = N'CriadoPor');
    IF @var26 IS NOT NULL EXEC(N'ALTER TABLE [Clientes] DROP CONSTRAINT ' + @var26 + ';');
    ALTER TABLE [Clientes] ALTER COLUMN [CriadoPor] nvarchar(254) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    DECLARE @var27 nvarchar(max);
    SELECT @var27 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clientes]') AND [c].[name] = N'CPF');
    IF @var27 IS NOT NULL EXEC(N'ALTER TABLE [Clientes] DROP CONSTRAINT ' + @var27 + ';');
    ALTER TABLE [Clientes] ALTER COLUMN [CPF] nchar(11) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    DECLARE @var28 nvarchar(max);
    SELECT @var28 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clientes]') AND [c].[name] = N'AtualizadoPor');
    IF @var28 IS NOT NULL EXEC(N'ALTER TABLE [Clientes] DROP CONSTRAINT ' + @var28 + ';');
    ALTER TABLE [Clientes] ALTER COLUMN [AtualizadoPor] nvarchar(254) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    CREATE INDEX [IX_Vendas_DataVenda] ON [Vendas] ([DataVenda]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Vendas_Numero] ON [Vendas] ([Numero]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Usuarios_Email] ON [Usuarios] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Produtos_Codigo] ON [Produtos] ([Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Clientes_CPF] ON [Clientes] ([CPF]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Clientes_Email] ON [Clientes] ([Email]) WHERE [Email] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    CREATE INDEX [IX_MovimentacoesFinanceiras_DataMovimentacao] ON [MovimentacoesFinanceiras] ([DataMovimentacao]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    CREATE INDEX [IX_MovimentacoesEstoque_CreatedAt] ON [MovimentacoesEstoque] ([CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826184821_MelhoriasArquiteturaESeguranca'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260826184821_MelhoriasArquiteturaESeguranca', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827134105_IdempotenciaEOperacaoSegura'
)
BEGIN
    CREATE TABLE [RegistrosIdempotencia] (
        [Id] uniqueidentifier NOT NULL,
        [Chave] nvarchar(100) NOT NULL,
        [VendaId] uniqueidentifier NOT NULL,
        [CriadoPor] nvarchar(254) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_RegistrosIdempotencia] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RegistrosIdempotencia_Vendas_VendaId] FOREIGN KEY ([VendaId]) REFERENCES [Vendas] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827134105_IdempotenciaEOperacaoSegura'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RegistrosIdempotencia_Chave] ON [RegistrosIdempotencia] ([Chave]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827134105_IdempotenciaEOperacaoSegura'
)
BEGIN
    CREATE INDEX [IX_RegistrosIdempotencia_CreatedAt] ON [RegistrosIdempotencia] ([CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827134105_IdempotenciaEOperacaoSegura'
)
BEGIN
    CREATE INDEX [IX_RegistrosIdempotencia_VendaId] ON [RegistrosIdempotencia] ([VendaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827134105_IdempotenciaEOperacaoSegura'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260827134105_IdempotenciaEOperacaoSegura', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827134316_HashDaRequisicaoIdempotente'
)
BEGIN
    ALTER TABLE [RegistrosIdempotencia] ADD [HashRequisicao] nchar(64) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827134316_HashDaRequisicaoIdempotente'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260827134316_HashDaRequisicaoIdempotente', N'10.0.0');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827141735_IndicesCompostosOperacionais'
)
BEGIN
    DROP INDEX [IX_MovimentacoesEstoque_ProdutoId] ON [MovimentacoesEstoque];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827141735_IndicesCompostosOperacionais'
)
BEGIN
    CREATE INDEX [IX_Vendas_Situacao_DataVenda] ON [Vendas] ([Situacao], [DataVenda]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827141735_IndicesCompostosOperacionais'
)
BEGIN
    CREATE INDEX [IX_MovimentacoesFinanceiras_TipoMovimentacao_DataMovimentacao] ON [MovimentacoesFinanceiras] ([TipoMovimentacao], [DataMovimentacao]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827141735_IndicesCompostosOperacionais'
)
BEGIN
    CREATE INDEX [IX_MovimentacoesEstoque_ProdutoId_CreatedAt] ON [MovimentacoesEstoque] ([ProdutoId], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260827141735_IndicesCompostosOperacionais'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260827141735_IndicesCompostosOperacionais', N'10.0.0');
END;

COMMIT;
GO
