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
                name: "acumulable",
                table: "items",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    username = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    monedas = table.Column<int>(type: "int", nullable: false),
                    spritekey = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    heroeseleccionadoid = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuario", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "inventarios",
                columns: table => new
                {
                    usuarioid = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    itemtiendaid = table.Column<int>(type: "int", nullable: false),
                    id = table.Column<int>(type: "int", nullable: false),
                    cantidad = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    equipado = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventarios", x => new { x.usuarioid, x.itemtiendaid });
                    table.ForeignKey(
                        name: "fk_inventarios_usuario_usuarioid",
                        column: x => x.usuarioid,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_inventarios_items_itemtiendaid",
                        column: x => x.itemtiendaid,
                        principalTable: "items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(table: "items", keyColumn: "Id", keyValue: 1, column: "Acumulable", value: false);
            migrationBuilder.UpdateData(table: "items", keyColumn: "Id", keyValue: 2, column: "Acumulable", value: false);
            migrationBuilder.UpdateData(table: "items", keyColumn: "Id", keyValue: 3, column: "Acumulable", value: false);
            migrationBuilder.UpdateData(table: "items", keyColumn: "Id", keyValue: 4, column: "Acumulable", value: false);
            migrationBuilder.UpdateData(table: "items", keyColumn: "Id", keyValue: 5, column: "Acumulable", value: false);
            migrationBuilder.UpdateData(table: "items", keyColumn: "Id", keyValue: 6, column: "Acumulable", value: false);
            migrationBuilder.UpdateData(table: "items", keyColumn: "Id", keyValue: 7, column: "Acumulable", value: false);
            migrationBuilder.UpdateData(table: "items", keyColumn: "Id", keyValue: 8, column: "Acumulable", value: false);

            migrationBuilder.CreateIndex(
                name: "ix_inventarios_itemtiendaid",
                table: "inventarios",
                column: "itemtiendaid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inventarios");

            migrationBuilder.DropTable(
                name: "usuario");

            migrationBuilder.DropColumn(
                name: "acumulable",
                table: "items");
        }
    }
}
