using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaDeGestaoComercial.Infraestrutura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class IdempotenciaEOperacaoSegura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegistrosIdempotencia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Chave = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VendaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CriadoPor = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosIdempotencia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrosIdempotencia_Vendas_VendaId",
                        column: x => x.VendaId,
                        principalTable: "Vendas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosIdempotencia_Chave",
                table: "RegistrosIdempotencia",
                column: "Chave",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosIdempotencia_CreatedAt",
                table: "RegistrosIdempotencia",
                column: "CreatedAt"
            );

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosIdempotencia_VendaId",
                table: "RegistrosIdempotencia",
                column: "VendaId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "RegistrosIdempotencia");
        }
    }
}
