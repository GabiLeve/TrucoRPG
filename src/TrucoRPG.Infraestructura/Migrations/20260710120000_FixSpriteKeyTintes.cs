using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrucoRPG.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class FixSpriteKeyTintes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "items",
                keyColumn: "id",
                keyValue: 5,
                column: "spritekey",
                value: "rosa");

            migrationBuilder.UpdateData(
                table: "items",
                keyColumn: "id",
                keyValue: 6,
                column: "spritekey",
                value: "marron");

            migrationBuilder.UpdateData(
                table: "items",
                keyColumn: "id",
                keyValue: 7,
                column: "spritekey",
                value: "rojo");

            migrationBuilder.UpdateData(
                table: "items",
                keyColumn: "id",
                keyValue: 8,
                column: "spritekey",
                value: "azul");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "items",
                keyColumn: "id",
                keyValue: 5,
                column: "spritekey",
                value: "personaje1rosa");

            migrationBuilder.UpdateData(
                table: "items",
                keyColumn: "id",
                keyValue: 6,
                column: "spritekey",
                value: "personaje1rosa");

            migrationBuilder.UpdateData(
                table: "items",
                keyColumn: "id",
                keyValue: 7,
                column: "spritekey",
                value: "personaje1rosa");

            migrationBuilder.UpdateData(
                table: "items",
                keyColumn: "id",
                keyValue: 8,
                column: "spritekey",
                value: "personaje1rosa");
        }
    }
}
