using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrucoRPG.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposArmarioYMonedas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Heroes_HeroeSeleccionadoId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventarios_AspNetUsers_UsuarioId",
                table: "Inventarios");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventarios_items_ItemTiendaId",
                table: "Inventarios");

            migrationBuilder.DropForeignKey(
                name: "FK_ProgresoPartida_AspNetUsers_UsuarioId",
                table: "ProgresoPartida");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Rivales",
                table: "Rivales");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProgresoPartida",
                table: "ProgresoPartida");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Inventarios",
                table: "Inventarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Heroes",
                table: "Heroes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserTokens",
                table: "AspNetUserTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUsers",
                table: "AspNetUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserRoles",
                table: "AspNetUserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserLogins",
                table: "AspNetUserLogins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserClaims",
                table: "AspNetUserClaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetRoles",
                table: "AspNetRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetRoleClaims",
                table: "AspNetRoleClaims");

            migrationBuilder.RenameTable(
                name: "Rivales",
                newName: "rivales");

            migrationBuilder.RenameTable(
                name: "ProgresoPartida",
                newName: "progresopartida");

            migrationBuilder.RenameTable(
                name: "Inventarios",
                newName: "inventarios");

            migrationBuilder.RenameTable(
                name: "Heroes",
                newName: "heroes");

            migrationBuilder.RenameTable(
                name: "AspNetUserTokens",
                newName: "aspnetusertokens");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                newName: "aspnetusers");

            migrationBuilder.RenameTable(
                name: "AspNetUserRoles",
                newName: "aspnetuserroles");

            migrationBuilder.RenameTable(
                name: "AspNetUserLogins",
                newName: "aspnetuserlogins");

            migrationBuilder.RenameTable(
                name: "AspNetUserClaims",
                newName: "aspnetuserclaims");

            migrationBuilder.RenameTable(
                name: "AspNetRoles",
                newName: "aspnetroles");

            migrationBuilder.RenameTable(
                name: "AspNetRoleClaims",
                newName: "aspnetroleclaims");

            migrationBuilder.RenameColumn(
                name: "TipoRival",
                table: "rivales",
                newName: "tiporival");

            migrationBuilder.RenameColumn(
                name: "TipoHabilidad",
                table: "rivales",
                newName: "tipohabilidad");

            migrationBuilder.RenameColumn(
                name: "NombreHabilidad",
                table: "rivales",
                newName: "nombrehabilidad");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "rivales",
                newName: "nombre");

            migrationBuilder.RenameColumn(
                name: "Nivel",
                table: "rivales",
                newName: "nivel");

            migrationBuilder.RenameColumn(
                name: "DescripcionHabilidad",
                table: "rivales",
                newName: "descripcionhabilidad");

            migrationBuilder.RenameColumn(
                name: "Descripcion",
                table: "rivales",
                newName: "descripcion");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "rivales",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_Rivales_Nivel",
                table: "rivales",
                newName: "IX_rivales_nivel");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "progresopartida",
                newName: "usuarioid");

            migrationBuilder.RenameColumn(
                name: "UltimoRivalDerrotadoNivel",
                table: "progresopartida",
                newName: "ultimorivalderrotadonivel");

            migrationBuilder.RenameColumn(
                name: "PuntosAcumulados",
                table: "progresopartida",
                newName: "puntosacumulados");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "progresopartida",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_ProgresoPartida_UsuarioId",
                table: "progresopartida",
                newName: "IX_progresopartida_usuarioid");

            migrationBuilder.RenameColumn(
                name: "SpriteKey",
                table: "items",
                newName: "spritekey");

            migrationBuilder.RenameColumn(
                name: "Precio",
                table: "items",
                newName: "precio");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "items",
                newName: "nombre");

            migrationBuilder.RenameColumn(
                name: "Img",
                table: "items",
                newName: "img");

            migrationBuilder.RenameColumn(
                name: "Descripcion",
                table: "items",
                newName: "descripcion");

            migrationBuilder.RenameColumn(
                name: "Categoria",
                table: "items",
                newName: "categoria");

            migrationBuilder.RenameColumn(
                name: "Acumulable",
                table: "items",
                newName: "acumulable");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "items",
                newName: "id");

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

            migrationBuilder.RenameColumn(
                name: "TipoHeroe",
                table: "heroes",
                newName: "tipoheroe");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "heroes",
                newName: "nombre");

            migrationBuilder.RenameColumn(
                name: "DescripcionHabilidadPasiva",
                table: "heroes",
                newName: "descripcionhabilidadpasiva");

            migrationBuilder.RenameColumn(
                name: "DescripcionHabilidadActiva",
                table: "heroes",
                newName: "descripcionhabilidadactiva");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "heroes",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "aspnetusertokens",
                newName: "value");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "aspnetusertokens",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "LoginProvider",
                table: "aspnetusertokens",
                newName: "loginprovider");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "aspnetusertokens",
                newName: "userid");

            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "aspnetusers",
                newName: "username");

            migrationBuilder.RenameColumn(
                name: "TwoFactorEnabled",
                table: "aspnetusers",
                newName: "twofactorenabled");

            migrationBuilder.RenameColumn(
                name: "SpriteKey",
                table: "aspnetusers",
                newName: "spritekey");

            migrationBuilder.RenameColumn(
                name: "SecurityStamp",
                table: "aspnetusers",
                newName: "securitystamp");

            migrationBuilder.RenameColumn(
                name: "PhoneNumberConfirmed",
                table: "aspnetusers",
                newName: "phonenumberconfirmed");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "aspnetusers",
                newName: "phonenumber");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "aspnetusers",
                newName: "passwordhash");

            migrationBuilder.RenameColumn(
                name: "NormalizedUserName",
                table: "aspnetusers",
                newName: "normalizedusername");

            migrationBuilder.RenameColumn(
                name: "NormalizedEmail",
                table: "aspnetusers",
                newName: "normalizedemail");

            migrationBuilder.RenameColumn(
                name: "Monedas",
                table: "aspnetusers",
                newName: "monedas");

            migrationBuilder.RenameColumn(
                name: "LockoutEnd",
                table: "aspnetusers",
                newName: "lockoutend");

            migrationBuilder.RenameColumn(
                name: "LockoutEnabled",
                table: "aspnetusers",
                newName: "lockoutenabled");

            migrationBuilder.RenameColumn(
                name: "HeroeSeleccionadoId",
                table: "aspnetusers",
                newName: "heroeseleccionadoid");

            migrationBuilder.RenameColumn(
                name: "EmailConfirmed",
                table: "aspnetusers",
                newName: "emailconfirmed");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "aspnetusers",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyStamp",
                table: "aspnetusers",
                newName: "concurrencystamp");

            migrationBuilder.RenameColumn(
                name: "AccessFailedCount",
                table: "aspnetusers",
                newName: "accessfailedcount");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "aspnetusers",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_HeroeSeleccionadoId",
                table: "aspnetusers",
                newName: "IX_aspnetusers_heroeseleccionadoid");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "aspnetuserroles",
                newName: "roleid");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "aspnetuserroles",
                newName: "userid");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "aspnetuserroles",
                newName: "IX_aspnetuserroles_roleid");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "aspnetuserlogins",
                newName: "userid");

            migrationBuilder.RenameColumn(
                name: "ProviderDisplayName",
                table: "aspnetuserlogins",
                newName: "providerdisplayname");

            migrationBuilder.RenameColumn(
                name: "ProviderKey",
                table: "aspnetuserlogins",
                newName: "providerkey");

            migrationBuilder.RenameColumn(
                name: "LoginProvider",
                table: "aspnetuserlogins",
                newName: "loginprovider");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "aspnetuserlogins",
                newName: "IX_aspnetuserlogins_userid");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "aspnetuserclaims",
                newName: "userid");

            migrationBuilder.RenameColumn(
                name: "ClaimValue",
                table: "aspnetuserclaims",
                newName: "claimvalue");

            migrationBuilder.RenameColumn(
                name: "ClaimType",
                table: "aspnetuserclaims",
                newName: "claimtype");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "aspnetuserclaims",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "aspnetuserclaims",
                newName: "IX_aspnetuserclaims_userid");

            migrationBuilder.RenameColumn(
                name: "NormalizedName",
                table: "aspnetroles",
                newName: "normalizedname");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "aspnetroles",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyStamp",
                table: "aspnetroles",
                newName: "concurrencystamp");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "aspnetroles",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "aspnetroleclaims",
                newName: "roleid");

            migrationBuilder.RenameColumn(
                name: "ClaimValue",
                table: "aspnetroleclaims",
                newName: "claimvalue");

            migrationBuilder.RenameColumn(
                name: "ClaimType",
                table: "aspnetroleclaims",
                newName: "claimtype");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "aspnetroleclaims",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "aspnetroleclaims",
                newName: "IX_aspnetroleclaims_roleid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_rivales",
                table: "rivales",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_progresopartida",
                table: "progresopartida",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_inventarios",
                table: "inventarios",
                columns: new[] { "usuarioid", "itemtiendaid" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_heroes",
                table: "heroes",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_aspnetusertokens",
                table: "aspnetusertokens",
                columns: new[] { "userid", "loginprovider", "name" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_aspnetusers",
                table: "aspnetusers",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_aspnetuserroles",
                table: "aspnetuserroles",
                columns: new[] { "userid", "roleid" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_aspnetuserlogins",
                table: "aspnetuserlogins",
                columns: new[] { "loginprovider", "providerkey" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_aspnetuserclaims",
                table: "aspnetuserclaims",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_aspnetroles",
                table: "aspnetroles",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_aspnetroleclaims",
                table: "aspnetroleclaims",
                column: "id");

            migrationBuilder.UpdateData(
                table: "rivales",
                keyColumn: "id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4"),
                columns: new[] { "descripcionhabilidad", "tipohabilidad" },
                values: new object[] { "Destello: cada 2 turnos en bazas 1 o 2, te obliga a jugar una carta al azar. Espejismo (pasiva): si es mano y abre la baza 1, muestra una carta falsa en pantalla hasta que respondas.", 5 });

            migrationBuilder.UpdateData(
                table: "rivales",
                keyColumn: "id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5"),
                columns: new[] { "descripcionhabilidad", "tipohabilidad" },
                values: new object[] { "Fase I (siempre): cada 2 manos maldice la mesa. Fase II (10+ pts tuyos): El Engaño. Fase III (20+ pts tuyos): El Espejo.", 6 });

            migrationBuilder.AddForeignKey(
                name: "FK_aspnetroleclaims_aspnetroles_roleid",
                table: "aspnetroleclaims",
                column: "roleid",
                principalTable: "aspnetroles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_aspnetuserclaims_aspnetusers_userid",
                table: "aspnetuserclaims",
                column: "userid",
                principalTable: "aspnetusers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_aspnetuserlogins_aspnetusers_userid",
                table: "aspnetuserlogins",
                column: "userid",
                principalTable: "aspnetusers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_aspnetuserroles_aspnetroles_roleid",
                table: "aspnetuserroles",
                column: "roleid",
                principalTable: "aspnetroles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_aspnetuserroles_aspnetusers_userid",
                table: "aspnetuserroles",
                column: "userid",
                principalTable: "aspnetusers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_aspnetusers_heroes_heroeseleccionadoid",
                table: "aspnetusers",
                column: "heroeseleccionadoid",
                principalTable: "heroes",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_aspnetusertokens_aspnetusers_userid",
                table: "aspnetusertokens",
                column: "userid",
                principalTable: "aspnetusers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_inventarios_aspnetusers_usuarioid",
                table: "inventarios",
                column: "usuarioid",
                principalTable: "aspnetusers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_inventarios_items_itemtiendaid",
                table: "inventarios",
                column: "itemtiendaid",
                principalTable: "items",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_progresopartida_aspnetusers_usuarioid",
                table: "progresopartida",
                column: "usuarioid",
                principalTable: "aspnetusers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_aspnetroleclaims_aspnetroles_roleid",
                table: "aspnetroleclaims");

            migrationBuilder.DropForeignKey(
                name: "FK_aspnetuserclaims_aspnetusers_userid",
                table: "aspnetuserclaims");

            migrationBuilder.DropForeignKey(
                name: "FK_aspnetuserlogins_aspnetusers_userid",
                table: "aspnetuserlogins");

            migrationBuilder.DropForeignKey(
                name: "FK_aspnetuserroles_aspnetroles_roleid",
                table: "aspnetuserroles");

            migrationBuilder.DropForeignKey(
                name: "FK_aspnetuserroles_aspnetusers_userid",
                table: "aspnetuserroles");

            migrationBuilder.DropForeignKey(
                name: "FK_aspnetusers_heroes_heroeseleccionadoid",
                table: "aspnetusers");

            migrationBuilder.DropForeignKey(
                name: "FK_aspnetusertokens_aspnetusers_userid",
                table: "aspnetusertokens");

            migrationBuilder.DropForeignKey(
                name: "FK_inventarios_aspnetusers_usuarioid",
                table: "inventarios");

            migrationBuilder.DropForeignKey(
                name: "FK_inventarios_items_itemtiendaid",
                table: "inventarios");

            migrationBuilder.DropForeignKey(
                name: "FK_progresopartida_aspnetusers_usuarioid",
                table: "progresopartida");

            migrationBuilder.DropPrimaryKey(
                name: "PK_rivales",
                table: "rivales");

            migrationBuilder.DropPrimaryKey(
                name: "PK_progresopartida",
                table: "progresopartida");

            migrationBuilder.DropPrimaryKey(
                name: "PK_inventarios",
                table: "inventarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_heroes",
                table: "heroes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_aspnetusertokens",
                table: "aspnetusertokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_aspnetusers",
                table: "aspnetusers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_aspnetuserroles",
                table: "aspnetuserroles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_aspnetuserlogins",
                table: "aspnetuserlogins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_aspnetuserclaims",
                table: "aspnetuserclaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_aspnetroles",
                table: "aspnetroles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_aspnetroleclaims",
                table: "aspnetroleclaims");

            migrationBuilder.RenameTable(
                name: "rivales",
                newName: "Rivales");

            migrationBuilder.RenameTable(
                name: "progresopartida",
                newName: "ProgresoPartida");

            migrationBuilder.RenameTable(
                name: "inventarios",
                newName: "Inventarios");

            migrationBuilder.RenameTable(
                name: "heroes",
                newName: "Heroes");

            migrationBuilder.RenameTable(
                name: "aspnetusertokens",
                newName: "AspNetUserTokens");

            migrationBuilder.RenameTable(
                name: "aspnetusers",
                newName: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "aspnetuserroles",
                newName: "AspNetUserRoles");

            migrationBuilder.RenameTable(
                name: "aspnetuserlogins",
                newName: "AspNetUserLogins");

            migrationBuilder.RenameTable(
                name: "aspnetuserclaims",
                newName: "AspNetUserClaims");

            migrationBuilder.RenameTable(
                name: "aspnetroles",
                newName: "AspNetRoles");

            migrationBuilder.RenameTable(
                name: "aspnetroleclaims",
                newName: "AspNetRoleClaims");

            migrationBuilder.RenameColumn(
                name: "tiporival",
                table: "Rivales",
                newName: "TipoRival");

            migrationBuilder.RenameColumn(
                name: "tipohabilidad",
                table: "Rivales",
                newName: "TipoHabilidad");

            migrationBuilder.RenameColumn(
                name: "nombrehabilidad",
                table: "Rivales",
                newName: "NombreHabilidad");

            migrationBuilder.RenameColumn(
                name: "nombre",
                table: "Rivales",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "nivel",
                table: "Rivales",
                newName: "Nivel");

            migrationBuilder.RenameColumn(
                name: "descripcionhabilidad",
                table: "Rivales",
                newName: "DescripcionHabilidad");

            migrationBuilder.RenameColumn(
                name: "descripcion",
                table: "Rivales",
                newName: "Descripcion");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Rivales",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_rivales_nivel",
                table: "Rivales",
                newName: "IX_Rivales_Nivel");

            migrationBuilder.RenameColumn(
                name: "usuarioid",
                table: "ProgresoPartida",
                newName: "UsuarioId");

            migrationBuilder.RenameColumn(
                name: "ultimorivalderrotadonivel",
                table: "ProgresoPartida",
                newName: "UltimoRivalDerrotadoNivel");

            migrationBuilder.RenameColumn(
                name: "puntosacumulados",
                table: "ProgresoPartida",
                newName: "PuntosAcumulados");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "ProgresoPartida",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_progresopartida_usuarioid",
                table: "ProgresoPartida",
                newName: "IX_ProgresoPartida_UsuarioId");

            migrationBuilder.RenameColumn(
                name: "spritekey",
                table: "items",
                newName: "SpriteKey");

            migrationBuilder.RenameColumn(
                name: "precio",
                table: "items",
                newName: "Precio");

            migrationBuilder.RenameColumn(
                name: "nombre",
                table: "items",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "img",
                table: "items",
                newName: "Img");

            migrationBuilder.RenameColumn(
                name: "descripcion",
                table: "items",
                newName: "Descripcion");

            migrationBuilder.RenameColumn(
                name: "categoria",
                table: "items",
                newName: "Categoria");

            migrationBuilder.RenameColumn(
                name: "acumulable",
                table: "items",
                newName: "Acumulable");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "items",
                newName: "Id");

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

            migrationBuilder.RenameColumn(
                name: "tipoheroe",
                table: "Heroes",
                newName: "TipoHeroe");

            migrationBuilder.RenameColumn(
                name: "nombre",
                table: "Heroes",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "descripcionhabilidadpasiva",
                table: "Heroes",
                newName: "DescripcionHabilidadPasiva");

            migrationBuilder.RenameColumn(
                name: "descripcionhabilidadactiva",
                table: "Heroes",
                newName: "DescripcionHabilidadActiva");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Heroes",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "value",
                table: "AspNetUserTokens",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "AspNetUserTokens",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "loginprovider",
                table: "AspNetUserTokens",
                newName: "LoginProvider");

            migrationBuilder.RenameColumn(
                name: "userid",
                table: "AspNetUserTokens",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "username",
                table: "AspNetUsers",
                newName: "UserName");

            migrationBuilder.RenameColumn(
                name: "twofactorenabled",
                table: "AspNetUsers",
                newName: "TwoFactorEnabled");

            migrationBuilder.RenameColumn(
                name: "spritekey",
                table: "AspNetUsers",
                newName: "SpriteKey");

            migrationBuilder.RenameColumn(
                name: "securitystamp",
                table: "AspNetUsers",
                newName: "SecurityStamp");

            migrationBuilder.RenameColumn(
                name: "phonenumberconfirmed",
                table: "AspNetUsers",
                newName: "PhoneNumberConfirmed");

            migrationBuilder.RenameColumn(
                name: "phonenumber",
                table: "AspNetUsers",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "passwordhash",
                table: "AspNetUsers",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "normalizedusername",
                table: "AspNetUsers",
                newName: "NormalizedUserName");

            migrationBuilder.RenameColumn(
                name: "normalizedemail",
                table: "AspNetUsers",
                newName: "NormalizedEmail");

            migrationBuilder.RenameColumn(
                name: "monedas",
                table: "AspNetUsers",
                newName: "Monedas");

            migrationBuilder.RenameColumn(
                name: "lockoutend",
                table: "AspNetUsers",
                newName: "LockoutEnd");

            migrationBuilder.RenameColumn(
                name: "lockoutenabled",
                table: "AspNetUsers",
                newName: "LockoutEnabled");

            migrationBuilder.RenameColumn(
                name: "heroeseleccionadoid",
                table: "AspNetUsers",
                newName: "HeroeSeleccionadoId");

            migrationBuilder.RenameColumn(
                name: "emailconfirmed",
                table: "AspNetUsers",
                newName: "EmailConfirmed");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "AspNetUsers",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "concurrencystamp",
                table: "AspNetUsers",
                newName: "ConcurrencyStamp");

            migrationBuilder.RenameColumn(
                name: "accessfailedcount",
                table: "AspNetUsers",
                newName: "AccessFailedCount");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "AspNetUsers",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_aspnetusers_heroeseleccionadoid",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_HeroeSeleccionadoId");

            migrationBuilder.RenameColumn(
                name: "roleid",
                table: "AspNetUserRoles",
                newName: "RoleId");

            migrationBuilder.RenameColumn(
                name: "userid",
                table: "AspNetUserRoles",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_aspnetuserroles_roleid",
                table: "AspNetUserRoles",
                newName: "IX_AspNetUserRoles_RoleId");

            migrationBuilder.RenameColumn(
                name: "userid",
                table: "AspNetUserLogins",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "providerdisplayname",
                table: "AspNetUserLogins",
                newName: "ProviderDisplayName");

            migrationBuilder.RenameColumn(
                name: "providerkey",
                table: "AspNetUserLogins",
                newName: "ProviderKey");

            migrationBuilder.RenameColumn(
                name: "loginprovider",
                table: "AspNetUserLogins",
                newName: "LoginProvider");

            migrationBuilder.RenameIndex(
                name: "IX_aspnetuserlogins_userid",
                table: "AspNetUserLogins",
                newName: "IX_AspNetUserLogins_UserId");

            migrationBuilder.RenameColumn(
                name: "userid",
                table: "AspNetUserClaims",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "claimvalue",
                table: "AspNetUserClaims",
                newName: "ClaimValue");

            migrationBuilder.RenameColumn(
                name: "claimtype",
                table: "AspNetUserClaims",
                newName: "ClaimType");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "AspNetUserClaims",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_aspnetuserclaims_userid",
                table: "AspNetUserClaims",
                newName: "IX_AspNetUserClaims_UserId");

            migrationBuilder.RenameColumn(
                name: "normalizedname",
                table: "AspNetRoles",
                newName: "NormalizedName");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "AspNetRoles",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "concurrencystamp",
                table: "AspNetRoles",
                newName: "ConcurrencyStamp");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "AspNetRoles",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "roleid",
                table: "AspNetRoleClaims",
                newName: "RoleId");

            migrationBuilder.RenameColumn(
                name: "claimvalue",
                table: "AspNetRoleClaims",
                newName: "ClaimValue");

            migrationBuilder.RenameColumn(
                name: "claimtype",
                table: "AspNetRoleClaims",
                newName: "ClaimType");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "AspNetRoleClaims",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_aspnetroleclaims_roleid",
                table: "AspNetRoleClaims",
                newName: "IX_AspNetRoleClaims_RoleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Rivales",
                table: "Rivales",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProgresoPartida",
                table: "ProgresoPartida",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Inventarios",
                table: "Inventarios",
                columns: new[] { "UsuarioId", "ItemTiendaId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Heroes",
                table: "Heroes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserTokens",
                table: "AspNetUserTokens",
                columns: new[] { "UserId", "LoginProvider", "Name" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUsers",
                table: "AspNetUsers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserRoles",
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserLogins",
                table: "AspNetUserLogins",
                columns: new[] { "LoginProvider", "ProviderKey" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserClaims",
                table: "AspNetUserClaims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetRoles",
                table: "AspNetRoles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetRoleClaims",
                table: "AspNetRoleClaims",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "Rivales",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4"),
                columns: new[] { "DescripcionHabilidad", "TipoHabilidad" },
                values: new object[] { "Emite una luz radiante que te confunde y te hace jugar una carta al azar (puede ocurrir en cualquier momento de la ronda).", 0 });

            migrationBuilder.UpdateData(
                table: "Rivales",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5"),
                columns: new[] { "DescripcionHabilidad", "TipoHabilidad" },
                values: new object[] { "Jefe final con 3 fases y distintas habilidades según los puntos que le quedan para ganar. (Próximamente.)", 0 });

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Heroes_HeroeSeleccionadoId",
                table: "AspNetUsers",
                column: "HeroeSeleccionadoId",
                principalTable: "Heroes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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

            migrationBuilder.AddForeignKey(
                name: "FK_ProgresoPartida_AspNetUsers_UsuarioId",
                table: "ProgresoPartida",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
