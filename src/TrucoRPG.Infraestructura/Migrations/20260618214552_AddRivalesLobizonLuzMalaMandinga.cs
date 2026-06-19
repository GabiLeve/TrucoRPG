using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TrucoRPG.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class AddRivalesLobizonLuzMalaMandinga : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Rivales",
                columns: new[] { "Id", "Descripcion", "DescripcionHabilidad", "Nivel", "Nombre", "NombreHabilidad", "TipoHabilidad", "TipoRival" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3"), "Tercer jefe de la historia. Acecha en las profundidades de la cueva.", "Combate sin habilidades especiales.", 3, "El Lobizón", "Sin habilidad", 0, 3 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4"), "Cuarto jefe de la historia. Una presencia luminosa que desorienta al viajero.", "Emite una luz radiante que te confunde y te hace jugar una carta al azar (puede ocurrir en cualquier momento de la ronda).", 4, "La Luz Mala", "Destello", 0, 4 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5"), "Jefe final de la historia. Domina el trono con tres fases de combate.", "Jefe final con 3 fases y distintas habilidades según los puntos que le quedan para ganar. (Próximamente.)", 5, "Mandinga", "Fases", 0, 5 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rivales",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3"));

            migrationBuilder.DeleteData(
                table: "Rivales",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4"));

            migrationBuilder.DeleteData(
                table: "Rivales",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5"));
        }
    }
}
