using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaDeGestaoComercial.Infraestrutura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class MensageriaRabbitMqOutboxInboxAlertas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertasEstoque",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProdutoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VendaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroVenda = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    QuantidadeAtual = table.Column<int>(type: "int", nullable: false),
                    EstoqueMinimo = table.Column<int>(type: "int", nullable: false),
                    Visualizado = table.Column<bool>(type: "bit", nullable: false),
                    VisualizadoAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CriadoPor = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertasEstoque", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertasEstoque_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_AlertasEstoque_Vendas_VendaId",
                        column: x => x.VendaId,
                        principalTable: "Vendas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "InboxMessages",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Consumer = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxMessages", x => new { x.MessageId, x.Consumer });
                }
            );

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Conteudo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Tentativas = table.Column<int>(type: "int", nullable: false),
                    Erro = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_AlertasEstoque_ProdutoId",
                table: "AlertasEstoque",
                column: "ProdutoId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_AlertasEstoque_VendaId_ProdutoId",
                table: "AlertasEstoque",
                columns: new[] { "VendaId", "ProdutoId" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedAt_CreatedAt",
                table: "OutboxMessages",
                columns: new[] { "ProcessedAt", "CreatedAt" }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AlertasEstoque");

            migrationBuilder.DropTable(name: "InboxMessages");

            migrationBuilder.DropTable(name: "OutboxMessages");
        }
    }
}
