using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrucoRPG.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePomberitoTravesura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Rivales",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2"),
                columns: new[] { "DescripcionHabilidad", "NombreHabilidad", "TipoHabilidad" },
                values: new object[] { "Cada 2 manos, te muestra tus 3 cartas 5 segundos y luego oculta 2 al azar.", "Travesura", 2 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Rivales",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2"),
                columns: new[] { "DescripcionHabilidad", "NombreHabilidad", "TipoHabilidad" },
                values: new object[] { "—", "—", 0 });
        }
    }
}
