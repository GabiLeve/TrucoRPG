using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TrucoRPG.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class ResetMinusculas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "aspnetroles",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    normalizedname = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    concurrencystamp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aspnetroles", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "heroes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    descripcionhabilidadpasiva = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    descripcionhabilidadactiva = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tipoheroe = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_heroes", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "items",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    descripcion = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    precio = table.Column<int>(type: "int", nullable: false),
                    categoria = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    img = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    spritekey = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_items", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "rivales",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    nivel = table.Column<int>(type: "int", nullable: false),
                    nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    descripcion = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nombrehabilidad = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    descripcionhabilidad = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tiporival = table.Column<int>(type: "int", nullable: false),
                    tipohabilidad = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rivales", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "aspnetroleclaims",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    roleid = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    claimtype = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    claimvalue = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aspnetroleclaims", x => x.id);
                    table.ForeignKey(
                        name: "FK_aspnetroleclaims_aspnetroles_roleid",
                        column: x => x.roleid,
                        principalTable: "aspnetroles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "aspnetusers",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    heroeseleccionadoid = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    username = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    normalizedusername = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    normalizedemail = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    emailconfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    passwordhash = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    securitystamp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    concurrencystamp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    phonenumber = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    phonenumberconfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    twofactorenabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    lockoutend = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    lockoutenabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    accessfailedcount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aspnetusers", x => x.id);
                    table.ForeignKey(
                        name: "FK_aspnetusers_heroes_heroeseleccionadoid",
                        column: x => x.heroeseleccionadoid,
                        principalTable: "heroes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "aspnetuserclaims",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    userid = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    claimtype = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    claimvalue = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aspnetuserclaims", x => x.id);
                    table.ForeignKey(
                        name: "FK_aspnetuserclaims_aspnetusers_userid",
                        column: x => x.userid,
                        principalTable: "aspnetusers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "aspnetuserlogins",
                columns: table => new
                {
                    loginprovider = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    providerkey = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    providerdisplayname = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    userid = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aspnetuserlogins", x => new { x.loginprovider, x.providerkey });
                    table.ForeignKey(
                        name: "FK_aspnetuserlogins_aspnetusers_userid",
                        column: x => x.userid,
                        principalTable: "aspnetusers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "aspnetuserroles",
                columns: table => new
                {
                    userid = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    roleid = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aspnetuserroles", x => new { x.userid, x.roleid });
                    table.ForeignKey(
                        name: "FK_aspnetuserroles_aspnetroles_roleid",
                        column: x => x.roleid,
                        principalTable: "aspnetroles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_aspnetuserroles_aspnetusers_userid",
                        column: x => x.userid,
                        principalTable: "aspnetusers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "aspnetusertokens",
                columns: table => new
                {
                    userid = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    loginprovider = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    value = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aspnetusertokens", x => new { x.userid, x.loginprovider, x.name });
                    table.ForeignKey(
                        name: "FK_aspnetusertokens_aspnetusers_userid",
                        column: x => x.userid,
                        principalTable: "aspnetusers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "progresopartida",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    usuarioid = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ultimorivalderrotadonivel = table.Column<int>(type: "int", nullable: false),
                    puntosacumulados = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_progresopartida", x => x.id);
                    table.ForeignKey(
                        name: "FK_progresopartida_aspnetusers_usuarioid",
                        column: x => x.usuarioid,
                        principalTable: "aspnetusers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "heroes",
                columns: new[] { "id", "descripcionhabilidadactiva", "descripcionhabilidadpasiva", "nombre", "tipoheroe" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Cada 3 manos, puede reemplazar una carta a eleccion de su mano por otra aleatoria del mazo. La nueva carta nunca puede ser de menor valor que la descartada.", "10% más de probabilidad de recibir carta de valor alto.", "Manipulador", 0 },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Debe activarse antes de comenzar la ronda. Si gana el truco, duplica los puntos de la ronda. Si pierde, el rival gana +2 puntos extra.", "El Timbero lanza una moneda:Cara → obtiene +1 punto. Cruz → no ocurre nada", "Timbero", 1 },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "El próximo Truco / Retruco / Vale 4 o Envido cantado por el Fanfarrón vale +1 punto adicional si es aceptado.", "En caso de empate de envido, en vez de definirse el ganador por quien es mano, el Fanfarrón gana automáticamente el empate.", "Fanfarron", 2 },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "Cada 2 manos revela UNA carta aleatoria del rival durante toda la ronda. Solo puede usarse al comienzo de la mano.", "El rival no puede ver cuándo El Mentiroso tiene habilidad disponible. Cuando usa su habilidad, el rival no recibe ninguna notificación visual.", "Mentiroso", 3 }
                });

            migrationBuilder.InsertData(
                table: "items",
                columns: new[] { "id", "categoria", "descripcion", "img", "nombre", "precio", "spritekey" },
                values: new object[,]
                {
                    { 1, "HABILIDADES", "Te otorga la habilidad del manipulador en una partida", "/assets/objetos/objeto.png", "BOLEADORAS", 150, null },
                    { 2, "HABILIDADES", "Te otorga la habilidad del timbero en una partida", "/assets/objetos/objeto.png", "BOLEADORAS", 150, null },
                    { 3, "HABILIDADES", "Te otorga la habilidad del fanfarron en una partida", "/assets/objetos/objeto.png", "BOLEADORAS", 150, null },
                    { 4, "HABILIDADES", "Te otorga la habilidad del mentiroso en una partida", "/assets/objetos/objeto.png", "BOLEADORAS", 150, null },
                    { 5, "ARMARIO", "Cambia el color de tu Poncho a rosa", "/assets/objetos/GotaRosa.png", "Poncho rosa", 150, "personaje1rosa" },
                    { 6, "ARMARIO", "Cambia el color de tu Poncho a marrón", "/assets/objetos/GotaMarron.png", "Poncho marrón", 150, "personaje1rosa" },
                    { 7, "ARMARIO", "Cambia el color de tu Poncho a rojo", "/assets/objetos/GotaRoja.png", "Poncho rojo", 150, "personaje1rosa" },
                    { 8, "ARMARIO", "Cambia el color de tu Poncho a azul", "/assets/objetos/GotaAzul.png", "Poncho azul", 150, "personaje1rosa" }
                });

            migrationBuilder.InsertData(
                table: "rivales",
                columns: new[] { "id", "descripcion", "descripcionhabilidad", "nivel", "nombre", "nombrehabilidad", "tipohabilidad", "tiporival" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"), "Primer jefe de la historia. Habita las orillas del lago.", "Cada 2 manos, cambia el palo de 2 cartas. Pasiva Remolino: 50% de cambiar el palo de tu primera carta en la 1.ª baza.", 1, "Nahuelito", "Salpicadura", 1, 1 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2"), "Segundo jefe de la historia. Guarda la entrada de la cueva.", "Cada 2 manos, muestra tus cartas 5s y oculta 2. Pasiva Trampa del monte: +1 pt si nadie cantó envido ni truco.", 2, "El Pomberito", "Travesura", 2, 2 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3"), "Tercer jefe de la historia. Acecha en las profundidades de la cueva.", "Rasguño: te cambia una carta aleatoria por una de menor valor (puede ocurrir en cualquier momento de la ronda).\nAullido: su aullido te asusta y te manda al mazo.", 3, "El Lobizón", "Rasguño", 3, 3 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4"), "Cuarto jefe de la historia. Una presencia luminosa que desorienta al viajero.", "Emite una luz radiante que te confunde y te hace jugar una carta al azar (puede ocurrir en cualquier momento de la ronda).", 4, "La Luz Mala", "Destello", 0, 4 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5"), "Jefe final de la historia. Domina el trono con tres fases de combate.", "Jefe final con 3 fases y distintas habilidades según los puntos que le quedan para ganar. (Próximamente.)", 5, "Mandinga", "Fases", 0, 5 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_aspnetroleclaims_roleid",
                table: "aspnetroleclaims",
                column: "roleid");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "aspnetroles",
                column: "normalizedname",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_aspnetuserclaims_userid",
                table: "aspnetuserclaims",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "IX_aspnetuserlogins_userid",
                table: "aspnetuserlogins",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "IX_aspnetuserroles_roleid",
                table: "aspnetuserroles",
                column: "roleid");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "aspnetusers",
                column: "normalizedemail");

            migrationBuilder.CreateIndex(
                name: "IX_aspnetusers_heroeseleccionadoid",
                table: "aspnetusers",
                column: "heroeseleccionadoid");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "aspnetusers",
                column: "normalizedusername",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_progresopartida_usuarioid",
                table: "progresopartida",
                column: "usuarioid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rivales_nivel",
                table: "rivales",
                column: "nivel",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aspnetroleclaims");

            migrationBuilder.DropTable(
                name: "aspnetuserclaims");

            migrationBuilder.DropTable(
                name: "aspnetuserlogins");

            migrationBuilder.DropTable(
                name: "aspnetuserroles");

            migrationBuilder.DropTable(
                name: "aspnetusertokens");

            migrationBuilder.DropTable(
                name: "items");

            migrationBuilder.DropTable(
                name: "progresopartida");

            migrationBuilder.DropTable(
                name: "rivales");

            migrationBuilder.DropTable(
                name: "aspnetroles");

            migrationBuilder.DropTable(
                name: "aspnetusers");

            migrationBuilder.DropTable(
                name: "heroes");
        }
    }
}
