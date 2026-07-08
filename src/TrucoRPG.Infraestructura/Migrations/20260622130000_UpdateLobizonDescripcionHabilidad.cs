using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrucoRPG.Infraestructura.Migrations
{
    /// <inheritdoc />
    [Migration("20260622130000_UpdateLobizonDescripcionHabilidad")]
    public partial class UpdateLobizonDescripcionHabilidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Rivales",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3"),
                column: "DescripcionHabilidad",
                value: "Rasguño: te cambia una carta aleatoria por una de menor valor (puede ocurrir en cualquier momento de la ronda).\nAullido: su aullido te asusta y te manda al mazo.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Rivales",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3"),
                column: "DescripcionHabilidad",
                value: "Cada 2 manos, debilita 1 carta. Pasivas: Luna llena al aceptar truco de la máquina; Aullido 20% tras ganar la 1.ª baza.");
        }
    }
}
