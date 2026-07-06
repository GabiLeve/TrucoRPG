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
            // 1. Borramos la clave foránea tradicional de EF usando sintaxis nativa de MySQL sin "IF EXISTS"
            migrationBuilder.DropForeignKey(
                name: "fk_inventarios_usuario_usuarioid", // Nombre en minúsculas como se creó originalmente
                table: "inventarios");

            // 2. Borramos la tabla vieja 'usuario'
            migrationBuilder.DropTable(
                name: "usuario");

            // 3. Añadimos la nueva clave foránea apuntando todo en minúsculas (a aspnetusers)
            migrationBuilder.AddForeignKey(
                name: "fk_inventarios_aspnetusers_usuarioid",
                table: "inventarios",
                column: "usuarioid",
                principalTable: "aspnetusers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revertimos la relación apuntando a minúsculas
            migrationBuilder.DropForeignKey(
                name: "fk_inventarios_aspnetusers_usuarioid",
                table: "inventarios");

            // Recreamos la tabla 'usuario' en minúsculas por si se hace un Rollback
            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    heroeseleccionadoid = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    monedas = table.Column<int>(type: "int", nullable: false),
                    spritekey = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    username = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuario", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // Recreamos la FK vieja apuntando a minúsculas
            migrationBuilder.AddForeignKey(
                name: "fk_inventarios_usuario_usuarioid",
                table: "inventarios",
                column: "usuarioid",
                principalTable: "usuario",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
