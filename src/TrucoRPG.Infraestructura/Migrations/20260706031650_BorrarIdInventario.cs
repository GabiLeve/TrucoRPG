using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrucoRPG.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class BorrarIdInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "Inventarios");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Inventarios",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
