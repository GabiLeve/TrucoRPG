using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TrucoRPG.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class AddHeroSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "HeroeSeleccionadoId",
                table: "AspNetUsers",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "Heroes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DescripcionHabilidadPasiva = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DescripcionHabilidadActiva = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TipoHeroe = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Heroes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Heroes",
                columns: new[] { "Id", "DescripcionHabilidadActiva", "DescripcionHabilidadPasiva", "Nombre", "TipoHeroe" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Cada 3 manos, puede reemplazar una carta a eleccion de su mano por otra aleatoria del mazo. La nueva carta nunca puede ser de menor valor que la descartada.", "10% más de probabilidad de recibir carta de valor alto.", "Manipulador", 0 },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Debe activarse antes de comenzar la ronda. Si gana el truco, duplica los puntos de la ronda. Si pierde, el rival gana +2 puntos extra.", "El Timbero lanza una moneda:Cara → obtiene +1 punto. Cruz → no ocurre nada", "Timbero", 1 },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "El próximo Truco / Retruco / Vale 4 o Envido cantado por el Fanfarrón vale +1 punto adicional si es aceptado.", "En caso de empate de envido, en vez de definirse el ganador por quien es mano, el Fanfarrón gana automáticamente el empate.", "Fanfarron", 2 },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "Cada 2 manos revela UNA carta aleatoria del rival durante toda la ronda. Solo puede usarse al comienzo de la mano.", "El rival no puede ver cuándo El Mentiroso tiene habilidad disponible. Cuando usa su habilidad, el rival no recibe ninguna notificación visual.", "Mentiroso", 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_HeroeSeleccionadoId",
                table: "AspNetUsers",
                column: "HeroeSeleccionadoId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Heroes_HeroeSeleccionadoId",
                table: "AspNetUsers",
                column: "HeroeSeleccionadoId",
                principalTable: "Heroes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Heroes_HeroeSeleccionadoId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Heroes");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_HeroeSeleccionadoId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "HeroeSeleccionadoId",
                table: "AspNetUsers");
        }
    }
}
