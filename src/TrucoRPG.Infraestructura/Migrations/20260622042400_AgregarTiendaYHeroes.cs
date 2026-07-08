using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TrucoRPG.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTiendaYHeroes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descripcion = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Precio = table.Column<int>(type: "int", nullable: false),
                    Categoria = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Img = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SpriteKey = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_items", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "items",
                columns: new[] { "Id", "Categoria", "Descripcion", "Img", "Nombre", "Precio", "SpriteKey" },
                values: new object[,]
                {
                    { 1, "HABILIDADES", "Te otorga la habilidad del manipulador en una partida", "/assets/objetos/habilidad-manipulador.png", "Habilidad manipulador", 150, null },
                    { 2, "HABILIDADES", "Te otorga la habilidad del timbero en una partida", "/assets/objetos/habilidad-timbero.png", "Habilidad timbero", 150, null },
                    { 3, "HABILIDADES", "Te otorga la habilidad del fanfarron en una partida", "/assets/objetos/habilidad-fanfarron.png", "Habilidad fanfarrón", 150, null },
                    { 4, "HABILIDADES", "Te otorga la habilidad del mentiroso en una partida", "/assets/objetos/habilidad-mentiroso.png", "Habilidad mentiroso", 150, null },
                    { 5, "ARMARIO", "Cambia el color de tu Poncho a rosa", "/assets/objetos/GotaRosa.png", "Poncho rosa", 150, "personaje1rosa" },
                    { 6, "ARMARIO", "Cambia el color de tu Poncho a marrón", "/assets/objetos/GotaMarron.png", "Poncho marrón", 150, "personaje1rosa" },
                    { 7, "ARMARIO", "Cambia el color de tu Poncho a rojo", "/assets/objetos/GotaRoja.png", "Poncho rojo", 150, "personaje1rosa" },
                    { 8, "ARMARIO", "Cambia el color de tu Poncho a azul", "/assets/objetos/GotaAzul.png", "Poncho azul", 150, "personaje1rosa" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "items");
        }
    }
}
