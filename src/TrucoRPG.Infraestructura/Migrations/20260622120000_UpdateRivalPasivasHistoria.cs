using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrucoRPG.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRivalPasivasHistoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Rivales",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"),
                column: "DescripcionHabilidad",
                value: "Cada 2 manos, cambia el palo de 2 cartas. Pasiva Remolino: 50% de cambiar el palo de tu primera carta en la 1.ª baza.");

            migrationBuilder.UpdateData(
                table: "Rivales",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2"),
                column: "DescripcionHabilidad",
                value: "Cada 2 manos, muestra tus cartas 5s y oculta 2. Pasiva Trampa del monte: +1 pt si nadie cantó envido ni truco.");

            migrationBuilder.UpdateData(
                table: "Rivales",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3"),
                column: "DescripcionHabilidad",
                value: "Cada 2 manos, debilita 1 carta. Pasivas: Luna llena al aceptar truco de la máquina; Aullido 20% tras ganar la 1.ª baza.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Rivales",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"),
                column: "DescripcionHabilidad",
                value: "Cada 2 manos, cambia los palos de tus cartas (ej. Espada se ve/vuelve Copa).");

            migrationBuilder.UpdateData(
                table: "Rivales",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2"),
                column: "DescripcionHabilidad",
                value: "Cada 2 manos, te muestra tus 3 cartas 5 segundos y luego oculta 2 al azar.");

            migrationBuilder.UpdateData(
                table: "Rivales",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3"),
                column: "DescripcionHabilidad",
                value: "Cada 2 manos, debilita 1 carta aleatoria bajando su valor en 1 (ej. 1 de Espada pasa a 1 de Basto, un 3 pasa a un 2).");
        }
    }
}
