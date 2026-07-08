using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrucoRPG.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class InicializarIdentityYModelos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_inventarios_AspNetUsers_usuarioid",
                table: "inventarios");

            migrationBuilder.DropForeignKey(
                name: "FK_inventarios_items_itemtiendaid",
                table: "inventarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_inventarios",
                table: "inventarios");

            migrationBuilder.RenameTable(
                name: "inventarios",
                newName: "Inventarios");

            migrationBuilder.RenameColumn(
                name: "acumulable",
                table: "items",
                newName: "Acumulable");

            migrationBuilder.RenameColumn(
                name: "equipado",
                table: "Inventarios",
                newName: "Equipado");

            migrationBuilder.RenameColumn(
                name: "cantidad",
                table: "Inventarios",
                newName: "Cantidad");

            migrationBuilder.RenameColumn(
                name: "itemtiendaid",
                table: "Inventarios",
                newName: "ItemTiendaId");

            migrationBuilder.RenameColumn(
                name: "usuarioid",
                table: "Inventarios",
                newName: "UsuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_inventarios_itemtiendaid",
                table: "Inventarios",
                newName: "IX_Inventarios_ItemTiendaId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Inventarios",
                table: "Inventarios",
                columns: new[] { "UsuarioId", "ItemTiendaId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Inventarios_AspNetUsers_UsuarioId",
                table: "Inventarios",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventarios_items_ItemTiendaId",
                table: "Inventarios",
                column: "ItemTiendaId",
                principalTable: "items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventarios_AspNetUsers_UsuarioId",
                table: "Inventarios");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventarios_items_ItemTiendaId",
                table: "Inventarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Inventarios",
                table: "Inventarios");

            migrationBuilder.RenameTable(
                name: "Inventarios",
                newName: "inventarios");

            migrationBuilder.RenameColumn(
                name: "Acumulable",
                table: "items",
                newName: "acumulable");

            migrationBuilder.RenameColumn(
                name: "Equipado",
                table: "inventarios",
                newName: "equipado");

            migrationBuilder.RenameColumn(
                name: "Cantidad",
                table: "inventarios",
                newName: "cantidad");

            migrationBuilder.RenameColumn(
                name: "ItemTiendaId",
                table: "inventarios",
                newName: "itemtiendaid");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "inventarios",
                newName: "usuarioid");

            migrationBuilder.RenameIndex(
                name: "IX_Inventarios_ItemTiendaId",
                table: "inventarios",
                newName: "IX_inventarios_itemtiendaid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_inventarios",
                table: "inventarios",
                columns: new[] { "usuarioid", "itemtiendaid" });

            migrationBuilder.AddForeignKey(
                name: "FK_inventarios_AspNetUsers_usuarioid",
                table: "inventarios",
                column: "usuarioid",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_inventarios_items_itemtiendaid",
                table: "inventarios",
                column: "itemtiendaid",
                principalTable: "items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
