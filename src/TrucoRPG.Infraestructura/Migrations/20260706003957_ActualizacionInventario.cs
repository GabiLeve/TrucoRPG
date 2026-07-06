using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrucoRPG.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class ActualizacionInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Acumulable",
                table: "items",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Monedas = table.Column<int>(type: "int", nullable: false),
                    SpriteKey = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HeroeSeleccionadoId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Inventarios",
                columns: table => new
                {
                    UsuarioId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ItemTiendaId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Equipado = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventarios", x => new { x.UsuarioId, x.ItemTiendaId });
                    table.ForeignKey(
                        name: "FK_Inventarios_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Inventarios_items_ItemTiendaId",
                        column: x => x.ItemTiendaId,
                        principalTable: "items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "items",
                keyColumn: "Id",
                keyValue: 1,
                column: "Acumulable",
                value: false);

            migrationBuilder.UpdateData(
                table: "items",
                keyColumn: "Id",
                keyValue: 2,
                column: "Acumulable",
                value: false);

            migrationBuilder.UpdateData(
                table: "items",
                keyColumn: "Id",
                keyValue: 3,
                column: "Acumulable",
                value: false);

            migrationBuilder.UpdateData(
                table: "items",
                keyColumn: "Id",
                keyValue: 4,
                column: "Acumulable",
                value: false);

            migrationBuilder.UpdateData(
                table: "items",
                keyColumn: "Id",
                keyValue: 5,
                column: "Acumulable",
                value: false);

            migrationBuilder.UpdateData(
                table: "items",
                keyColumn: "Id",
                keyValue: 6,
                column: "Acumulable",
                value: false);

            migrationBuilder.UpdateData(
                table: "items",
                keyColumn: "Id",
                keyValue: 7,
                column: "Acumulable",
                value: false);

            migrationBuilder.UpdateData(
                table: "items",
                keyColumn: "Id",
                keyValue: 8,
                column: "Acumulable",
                value: false);

            migrationBuilder.CreateIndex(
                name: "IX_Inventarios_ItemTiendaId",
                table: "Inventarios",
                column: "ItemTiendaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Inventarios");

            migrationBuilder.DropTable(
                name: "Usuario");

            migrationBuilder.DropColumn(
                name: "Acumulable",
                table: "items");
        }
    }
}
