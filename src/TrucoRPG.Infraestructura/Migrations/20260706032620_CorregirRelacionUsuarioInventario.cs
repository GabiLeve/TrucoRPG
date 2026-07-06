using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrucoRPG.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class CorregirRelacionUsuarioInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Borramos la FK usando SQL seguro con IF EXISTS por si acaso
            migrationBuilder.Sql("ALTER TABLE `inventarios` DROP FOREIGN KEY IF EXISTS `FK_Inventarios_Usuario_UsuarioId`;");
            migrationBuilder.Sql("ALTER TABLE `inventarios` DROP FOREIGN KEY IF EXISTS `fk_inventarios_usuario_usuarioid`;");

            // Borramos la tabla vieja en minúsculas
            migrationBuilder.DropTable(name: "usuario");

            // Creamos la nueva relación apuntando todo a las tablas en minúsculas
            migrationBuilder.AddForeignKey(
                name: "fk_inventarios_aspnetusers_usuarioid",
                table: "inventarios",       // <-- Minúsculas
                column: "UsuarioId",        // Tu propiedad de C# (esta se mapea igual)
                principalTable: "aspnetusers", // <-- Minúsculas (Identity en Linux)
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventarios_AspNetUsers_UsuarioId",
                table: "Inventarios");

            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HeroeSeleccionadoId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Monedas = table.Column<int>(type: "int", nullable: false),
                    SpriteKey = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventarios_Usuario_UsuarioId",
                table: "Inventarios",
                column: "UsuarioId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
