using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaDeGestaoComercial.Infraestrutura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class MelhoriasArquiteturaESeguranca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(name: "NumeroVendaSequence");

            migrationBuilder.DropIndex(name: "IX_Vendas_Numero", table: "Vendas");
            migrationBuilder.DropIndex(name: "IX_Usuarios_Email", table: "Usuarios");
            migrationBuilder.DropIndex(name: "IX_Produtos_Codigo", table: "Produtos");
            migrationBuilder.DropIndex(name: "IX_Clientes_CPF", table: "Clientes");
            migrationBuilder.DropIndex(name: "IX_Clientes_Email", table: "Clientes");

            migrationBuilder.AlterColumn<string>(
                name: "Numero",
                table: "Vendas",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)"
            );

            migrationBuilder.AlterColumn<string>(
                name: "CriadoPor",
                table: "Vendas",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)"
            );

            migrationBuilder.AlterColumn<string>(
                name: "SenhaHash",
                table: "Usuarios",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)"
            );

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Usuarios",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)"
            );

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Usuarios",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)"
            );

            migrationBuilder.AlterColumn<string>(
                name: "CriadoPor",
                table: "Usuarios",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)"
            );

            migrationBuilder.AlterColumn<string>(
                name: "AtualizadoPor",
                table: "Usuarios",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true
            );

            migrationBuilder.AddColumn<int>(
                name: "VersaoToken",
                table: "Usuarios",
                type: "int",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Produtos",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)"
            );

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "Produtos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "CriadoPor",
                table: "Produtos",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)"
            );

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "Produtos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)"
            );

            migrationBuilder.AlterColumn<string>(
                name: "AtualizadoPor",
                table: "Produtos",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true
            );

            migrationBuilder.AddColumn<byte[]>(
                name: "Versao",
                table: "Produtos",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]
            );

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "MovimentacoesFinanceiras",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)"
            );

            migrationBuilder.AlterColumn<string>(
                name: "CriadoPor",
                table: "MovimentacoesFinanceiras",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)"
            );

            migrationBuilder.AlterColumn<string>(
                name: "Observacao",
                table: "MovimentacoesEstoque",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "CriadoPor",
                table: "MovimentacoesEstoque",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)"
            );

            migrationBuilder.AlterColumn<string>(
                name: "Telefone",
                table: "Clientes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)"
            );

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Clientes",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)"
            );

            migrationBuilder.AlterColumn<string>(
                name: "Endereco_UF",
                table: "Clientes",
                type: "nchar(2)",
                fixedLength: true,
                maxLength: 2,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)"
            );

            migrationBuilder.AlterColumn<string>(
                name: "Endereco_Numero",
                table: "Clientes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)"
            );

            migrationBuilder.AlterColumn<string>(
                name: "Endereco_Logradouro",
                table: "Clientes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)"
            );

            migrationBuilder.AlterColumn<string>(
                name: "Endereco_Complemento",
                table: "Clientes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Endereco_Cidade",
                table: "Clientes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)"
            );

            migrationBuilder.AlterColumn<string>(
                name: "Endereco_CEP",
                table: "Clientes",
                type: "nchar(8)",
                fixedLength: true,
                maxLength: 8,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)"
            );

            migrationBuilder.AlterColumn<string>(
                name: "Endereco_Bairro",
                table: "Clientes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)"
            );

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Clientes",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "CriadoPor",
                table: "Clientes",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)"
            );

            migrationBuilder.AlterColumn<string>(
                name: "CPF",
                table: "Clientes",
                type: "nchar(11)",
                fixedLength: true,
                maxLength: 11,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)"
            );

            migrationBuilder.AlterColumn<string>(
                name: "AtualizadoPor",
                table: "Clientes",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true
            );

            migrationBuilder.CreateIndex(name: "IX_Vendas_DataVenda", table: "Vendas", column: "DataVenda");

            migrationBuilder.CreateIndex(name: "IX_Vendas_Numero", table: "Vendas", column: "Numero", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Usuarios_Email", table: "Usuarios", column: "Email", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Produtos_Codigo", table: "Produtos", column: "Codigo", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Clientes_CPF", table: "Clientes", column: "CPF", unique: true);
            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Email",
                table: "Clientes",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL"
            );

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesFinanceiras_DataMovimentacao",
                table: "MovimentacoesFinanceiras",
                column: "DataMovimentacao"
            );

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_CreatedAt",
                table: "MovimentacoesEstoque",
                column: "CreatedAt"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Vendas_Numero", table: "Vendas");
            migrationBuilder.DropIndex(name: "IX_Usuarios_Email", table: "Usuarios");
            migrationBuilder.DropIndex(name: "IX_Produtos_Codigo", table: "Produtos");
            migrationBuilder.DropIndex(name: "IX_Clientes_CPF", table: "Clientes");
            migrationBuilder.DropIndex(name: "IX_Clientes_Email", table: "Clientes");

            migrationBuilder.DropIndex(name: "IX_Vendas_DataVenda", table: "Vendas");

            migrationBuilder.DropIndex(
                name: "IX_MovimentacoesFinanceiras_DataMovimentacao",
                table: "MovimentacoesFinanceiras"
            );

            migrationBuilder.DropIndex(name: "IX_MovimentacoesEstoque_CreatedAt", table: "MovimentacoesEstoque");

            migrationBuilder.DropColumn(name: "VersaoToken", table: "Usuarios");

            migrationBuilder.DropColumn(name: "Versao", table: "Produtos");

            migrationBuilder.DropSequence(name: "NumeroVendaSequence");

            migrationBuilder.AlterColumn<string>(
                name: "Numero",
                table: "Vendas",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30
            );

            migrationBuilder.AlterColumn<string>(
                name: "CriadoPor",
                table: "Vendas",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(254)",
                oldMaxLength: 254
            );

            migrationBuilder.AlterColumn<string>(
                name: "SenhaHash",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500
            );

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150
            );

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Usuarios",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(254)",
                oldMaxLength: 254
            );

            migrationBuilder.AlterColumn<string>(
                name: "CriadoPor",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(254)",
                oldMaxLength: 254
            );

            migrationBuilder.AlterColumn<string>(
                name: "AtualizadoPor",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(254)",
                oldMaxLength: 254,
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Produtos",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150
            );

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "Produtos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "CriadoPor",
                table: "Produtos",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(254)",
                oldMaxLength: 254
            );

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "Produtos",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50
            );

            migrationBuilder.AlterColumn<string>(
                name: "AtualizadoPor",
                table: "Produtos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(254)",
                oldMaxLength: 254,
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "MovimentacoesFinanceiras",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500
            );

            migrationBuilder.AlterColumn<string>(
                name: "CriadoPor",
                table: "MovimentacoesFinanceiras",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(254)",
                oldMaxLength: 254
            );

            migrationBuilder.AlterColumn<string>(
                name: "Observacao",
                table: "MovimentacoesEstoque",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "CriadoPor",
                table: "MovimentacoesEstoque",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(254)",
                oldMaxLength: 254
            );

            migrationBuilder.AlterColumn<string>(
                name: "Telefone",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20
            );

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150
            );

            migrationBuilder.AlterColumn<string>(
                name: "Endereco_UF",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nchar(2)",
                oldFixedLength: true,
                oldMaxLength: 2
            );

            migrationBuilder.AlterColumn<string>(
                name: "Endereco_Numero",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20
            );

            migrationBuilder.AlterColumn<string>(
                name: "Endereco_Logradouro",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200
            );

            migrationBuilder.AlterColumn<string>(
                name: "Endereco_Complemento",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Endereco_Cidade",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100
            );

            migrationBuilder.AlterColumn<string>(
                name: "Endereco_CEP",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nchar(8)",
                oldFixedLength: true,
                oldMaxLength: 8
            );

            migrationBuilder.AlterColumn<string>(
                name: "Endereco_Bairro",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100
            );

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Clientes",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(254)",
                oldMaxLength: 254,
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "CriadoPor",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(254)",
                oldMaxLength: 254
            );

            migrationBuilder.AlterColumn<string>(
                name: "CPF",
                table: "Clientes",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nchar(11)",
                oldFixedLength: true,
                oldMaxLength: 11
            );

            migrationBuilder.AlterColumn<string>(
                name: "AtualizadoPor",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(254)",
                oldMaxLength: 254,
                oldNullable: true
            );

            migrationBuilder.CreateIndex(name: "IX_Vendas_Numero", table: "Vendas", column: "Numero", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Usuarios_Email", table: "Usuarios", column: "Email", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Produtos_Codigo", table: "Produtos", column: "Codigo", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Clientes_CPF", table: "Clientes", column: "CPF", unique: true);
            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Email",
                table: "Clientes",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL"
            );
        }
    }
}
