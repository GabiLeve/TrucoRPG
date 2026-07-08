using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrucoRPG.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class AddRivalAndProgresoPartida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Rivales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Nivel = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descripcion = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NombreHabilidad = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DescripcionHabilidad = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TipoRival = table.Column<int>(type: "int", nullable: false),
                    TipoHabilidad = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rivales", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProgresoPartida",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UsuarioId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UltimoRivalDerrotadoNivel = table.Column<int>(type: "int", nullable: false),
                    PuntosAcumulados = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgresoPartida", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgresoPartida_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Rivales",
                columns: new[] { "Id", "Descripcion", "DescripcionHabilidad", "Nivel", "Nombre", "NombreHabilidad", "TipoHabilidad", "TipoRival" },
                values: new object[] { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"), "Primer jefe de la historia. Habita las orillas del lago.", "Cada 2 manos, cambia los palos de tus cartas (ej. Espada se ve/vuelve Copa).", 1, "Nahuelito", "Salpicadura", 1, 1 });

            migrationBuilder.CreateIndex(
                name: "IX_ProgresoPartida_UsuarioId",
                table: "ProgresoPartida",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rivales_Nivel",
                table: "Rivales",
                column: "Nivel",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProgresoPartida");

            migrationBuilder.DropTable(
                name: "Rivales");
        }
    }
}
