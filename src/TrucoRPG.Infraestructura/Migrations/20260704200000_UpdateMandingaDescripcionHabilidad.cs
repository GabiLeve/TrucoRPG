using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrucoRPG.Infraestructura.Migrations
{
    /// <inheritdoc />
    [Migration("20260704200000_UpdateMandingaDescripcionHabilidad")]
    public partial class UpdateMandingaDescripcionHabilidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Rivales",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5"),
                columns: new[] { "DescripcionHabilidad", "TipoHabilidad" },
                values: new object[]
                {
                    "Fase I (siempre): cada 2 manos maldice la mesa. Fase II (10+ pts tuyos): El Engaño. Fase III (20+ pts tuyos): El Espejo.",
                    6
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Rivales",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5"),
                columns: new[] { "DescripcionHabilidad", "TipoHabilidad" },
                values: new object[]
                {
                    "Jefe final con 3 fases y distintas habilidades según los puntos que le quedan para ganar. (Próximamente.)",
                    0
                });
        }
    }
}
