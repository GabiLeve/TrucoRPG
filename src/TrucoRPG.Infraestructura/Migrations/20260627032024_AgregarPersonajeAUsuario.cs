using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrucoRPG.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class AgregarPersonajeAUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Monedas",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SpriteKey",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Monedas",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SpriteKey",
                table: "AspNetUsers");
        }
    }
}
