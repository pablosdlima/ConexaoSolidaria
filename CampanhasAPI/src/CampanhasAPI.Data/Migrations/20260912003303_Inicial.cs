using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampanhasAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "campanhas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DataInicio = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DataFim = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    MetaFinanceira = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DataCriacao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_campanhas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NomeCompleto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    Cpf = table.Column<string>(type: "character(11)", fixedLength: true, maxLength: 11, nullable: false),
                    SenhaHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DataCriacao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "doacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdCampanha = table.Column<Guid>(type: "uuid", nullable: false),
                    IdDoador = table.Column<Guid>(type: "uuid", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DataHora = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_doacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_doacoes_campanhas_IdCampanha",
                        column: x => x.IdCampanha,
                        principalTable: "campanhas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_doacoes_usuarios_IdDoador",
                        column: x => x.IdDoador,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_campanhas_Status",
                table: "campanhas",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_doacoes_IdCampanha",
                table: "doacoes",
                column: "IdCampanha");

            migrationBuilder.CreateIndex(
                name: "IX_doacoes_IdDoador",
                table: "doacoes",
                column: "IdDoador");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_Cpf",
                table: "usuarios",
                column: "Cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_Email",
                table: "usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "doacoes");

            migrationBuilder.DropTable(
                name: "campanhas");

            migrationBuilder.DropTable(
                name: "usuarios");
        }
    }
}
