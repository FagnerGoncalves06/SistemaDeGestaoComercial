USE [GestaoComercial];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Email nvarchar(450) = N'admin@gestao.test';
DECLARE @Nome nvarchar(max) = N'Administrador';
-- Execute com sqlcmd informando um hash PBKDF2 gerado pela aplicação:
-- sqlcmd -S servidor -d GestaoComercial -v SENHA_HASH_ADMIN="<hash>" -i criar-usuario-admin.sql
DECLARE @SenhaHash nvarchar(500) = N'$(SENHA_HASH_ADMIN)';

IF @SenhaHash = N'' OR @SenhaHash = N'$(SENHA_HASH_ADMIN)'
    THROW 50001, 'Informe SENHA_HASH_ADMIN externamente. Nenhuma credencial é versionada neste script.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    IF EXISTS (SELECT 1 FROM [dbo].[Usuarios] WHERE [Email] = @Email)
    BEGIN
        UPDATE [dbo].[Usuarios]
        SET [Nome] = @Nome,
            [SenhaHash] = @SenhaHash,
            [Perfil] = 0,
            [Ativo] = 1,
            [UpdatedAt] = SYSUTCDATETIME(),
            [AtualizadoPor] = N'script-criar-usuario-admin'
        WHERE [Email] = @Email;

        PRINT N'Usuário administrador atualizado e reativado.';
    END
    ELSE
    BEGIN
        INSERT INTO [dbo].[Usuarios]
        (
            [Id],
            [Nome],
            [Email],
            [SenhaHash],
            [Perfil],
            [Ativo],
            [CreatedAt],
            [CriadoPor],
            [UpdatedAt],
            [AtualizadoPor]
        )
        VALUES
        (
            NEWID(),
            @Nome,
            @Email,
            @SenhaHash,
            0,
            1,
            SYSUTCDATETIME(),
            N'script-criar-usuario-admin',
            NULL,
            NULL
        );

        PRINT N'Usuário administrador criado.';
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

SELECT [Id], [Nome], [Email], [Perfil], [Ativo]
FROM [dbo].[Usuarios]
WHERE [Email] = N'admin@gestao.test';
GO
