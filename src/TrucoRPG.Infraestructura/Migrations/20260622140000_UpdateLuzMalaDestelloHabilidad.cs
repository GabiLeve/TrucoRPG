using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrucoRPG.Infraestructura.Migrations
{
    /// <inheritdoc />
    [Migration("20260622140000_UpdateLuzMalaDestelloHabilidad")]
    public partial class UpdateLuzMalaDestelloHabilidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Rivales",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4"),
                columns: new[] { "DescripcionHabilidad", "TipoHabilidad" },
                values: new object[]
                {
                    "Cada 2 turnos en bazas 1 o 2, te confunde y te obliga a jugar una carta al azar de tu mano.",
                    5
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Rivales",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4"),
                columns: new[] { "DescripcionHabilidad", "TipoHabilidad" },
                values: new object[]
                {
                    "Emite una luz radiante que te confunde y te hace jugar una carta al azar (puede ocurrir en cualquier momento de la ronda).",
                    0
                });
        }
    }
}
