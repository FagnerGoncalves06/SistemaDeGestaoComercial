using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaDeGestaoComercial.Infraestrutura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class IndicesCompostosOperacionais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_MovimentacoesEstoque_ProdutoId", table: "MovimentacoesEstoque");

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_Situacao_DataVenda",
                table: "Vendas",
                columns: new[] { "Situacao", "DataVenda" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesFinanceiras_TipoMovimentacao_DataMovimentacao",
                table: "MovimentacoesFinanceiras",
                columns: new[] { "TipoMovimentacao", "DataMovimentacao" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_ProdutoId_CreatedAt",
                table: "MovimentacoesEstoque",
                columns: new[] { "ProdutoId", "CreatedAt" }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Vendas_Situacao_DataVenda", table: "Vendas");

            migrationBuilder.DropIndex(
                name: "IX_MovimentacoesFinanceiras_TipoMovimentacao_DataMovimentacao",
                table: "MovimentacoesFinanceiras"
            );

            migrationBuilder.DropIndex(
                name: "IX_MovimentacoesEstoque_ProdutoId_CreatedAt",
                table: "MovimentacoesEstoque"
            );

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_ProdutoId",
                table: "MovimentacoesEstoque",
                column: "ProdutoId"
            );
        }
    }
}
