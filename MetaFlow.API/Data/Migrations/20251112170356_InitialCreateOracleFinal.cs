using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetaFlow.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateOracleFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "USUARIOS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    NOME = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    EMAIL = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    SENHA_HASH = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: false),
                    PROFISSAO = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    OBJETIVO_PROFISSIONAL = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    CRIADO_EM = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false, defaultValueSql: "SYSTIMESTAMP"),
                    ATUALIZADO_EM = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false, defaultValueSql: "SYSTIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USUARIOS", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "METAS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    USUARIO_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TITULO = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    CATEGORIA = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    PRAZO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    PROGRESSO = table.Column<decimal>(type: "NUMBER(5,2)", nullable: false),
                    DESCRICAO = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    CRIADO_EM = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false, defaultValueSql: "SYSTIMESTAMP"),
                    STATUS = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_METAS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_METAS_USUARIO",
                        column: x => x.USUARIO_ID,
                        principalTable: "USUARIOS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "REGISTROS_DIARIOS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    USUARIO_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    DATA = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    HUMOR = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    PRODUTIVIDADE = table.Column<byte>(type: "NUMBER(3)", nullable: false),
                    TEMPO_FOCO = table.Column<short>(type: "NUMBER(5)", nullable: false, defaultValue: (short)0),
                    ANOTACOES = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    CRIADO_EM = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false, defaultValueSql: "SYSTIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REGISTROS_DIARIOS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_REGISTROS_USUARIO",
                        column: x => x.USUARIO_ID,
                        principalTable: "USUARIOS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RESUMOS_MENSAIS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    USUARIO_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ANO = table.Column<byte>(type: "NUMBER(4)", nullable: false),
                    MES = table.Column<byte>(type: "NUMBER(2)", nullable: false),
                    TOTAL_REGISTROS = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    METAS_CONCLUIDAS = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    MEDIA_HUMOR = table.Column<decimal>(type: "NUMBER(4,2)", nullable: false),
                    MEDIA_PRODUTIVIDADE = table.Column<decimal>(type: "NUMBER(4,2)", nullable: false),
                    TAXA_CONCLUSAO = table.Column<decimal>(type: "NUMBER(5,2)", nullable: false),
                    CALCULADO_EM = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false, defaultValueSql: "SYSTIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RESUMOS_MENSAIS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_RESUMOS_USUARIO",
                        column: x => x.USUARIO_ID,
                        principalTable: "USUARIOS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_METAS_CATEGORIA",
                table: "METAS",
                column: "CATEGORIA");

            migrationBuilder.CreateIndex(
                name: "IX_METAS_PRAZO",
                table: "METAS",
                column: "PRAZO");

            migrationBuilder.CreateIndex(
                name: "IX_METAS_USUARIO_STATUS",
                table: "METAS",
                columns: new[] { "USUARIO_ID", "STATUS" });

            migrationBuilder.CreateIndex(
                name: "IX_REGISTROS_DATA",
                table: "REGISTROS_DIARIOS",
                column: "DATA");

            migrationBuilder.CreateIndex(
                name: "IX_REGISTROS_USUARIO_DATA",
                table: "REGISTROS_DIARIOS",
                columns: new[] { "USUARIO_ID", "DATA" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RESUMOS_USUARIO_PERIODO",
                table: "RESUMOS_MENSAIS",
                columns: new[] { "USUARIO_ID", "ANO", "MES" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USUARIOS_EMAIL",
                table: "USUARIOS",
                column: "EMAIL",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "METAS");

            migrationBuilder.DropTable(
                name: "REGISTROS_DIARIOS");

            migrationBuilder.DropTable(
                name: "RESUMOS_MENSAIS");

            migrationBuilder.DropTable(
                name: "USUARIOS");
        }
    }
}
