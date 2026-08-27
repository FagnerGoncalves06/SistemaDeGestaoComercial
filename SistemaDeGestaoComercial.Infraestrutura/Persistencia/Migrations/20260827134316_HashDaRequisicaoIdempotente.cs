using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaDeGestaoComercial.Infraestrutura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class HashDaRequisicaoIdempotente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HashRequisicao",
                table: "RegistrosIdempotencia",
                type: "nchar(64)",
                fixedLength: true,
                maxLength: 64,
                nullable: false,
                defaultValue: ""
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "HashRequisicao", table: "RegistrosIdempotencia");
        }
    }
}
