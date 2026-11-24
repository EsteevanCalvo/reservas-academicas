using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reservas.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class FlujoDeReservas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "reservas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EspacioId = table.Column<int>(type: "INTEGER", nullable: false),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    Inicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Fin = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Estado = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    MotivoRechazo = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    MotivoCancelacion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreadaEn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DecididaEn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DecididaPorId = table.Column<int>(type: "INTEGER", nullable: true),
                    CanceladaEn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CanceladaPorId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reservas_espacios_EspacioId",
                        column: x => x.EspacioId,
                        principalTable: "espacios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reservas_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "franjas_reservadas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReservaId = table.Column<int>(type: "INTEGER", nullable: false),
                    EspacioId = table.Column<int>(type: "INTEGER", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Indice = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_franjas_reservadas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_franjas_reservadas_espacios_EspacioId",
                        column: x => x.EspacioId,
                        principalTable: "espacios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_franjas_reservadas_reservas_ReservaId",
                        column: x => x.ReservaId,
                        principalTable: "reservas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_franjas_reservadas_ReservaId",
                table: "franjas_reservadas",
                column: "ReservaId");

            migrationBuilder.CreateIndex(
                name: "IX_franjas_reservadas_unicas",
                table: "franjas_reservadas",
                columns: new[] { "EspacioId", "Fecha", "Indice" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reservas_EspacioId_Inicio",
                table: "reservas",
                columns: new[] { "EspacioId", "Inicio" });

            migrationBuilder.CreateIndex(
                name: "IX_reservas_UsuarioId_Inicio",
                table: "reservas",
                columns: new[] { "UsuarioId", "Inicio" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "franjas_reservadas");

            migrationBuilder.DropTable(
                name: "reservas");
        }
    }
}
