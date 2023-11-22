using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CryptoBank.Migrations
{
    /// <inheritdoc />
    public partial class BitcoinDepositAddressGeneration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "currencies",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    name = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_currencies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "variables",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    key = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    value = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_variables", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "xpubs",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_xpubs", x => x.id);
                    table.ForeignKey(
                        name: "FK_xpubs_currencies_currency_id",
                        column: x => x.currency_id,
                        principalSchema: "public",
                        principalTable: "currencies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "deposit_addresses",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    xpub_id = table.Column<int>(type: "integer", nullable: false),
                    derivation_index = table.Column<int>(type: "integer", nullable: false),
                    crypto_address = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deposit_addresses", x => x.id);
                    table.ForeignKey(
                        name: "FK_deposit_addresses_currencies_currency_id",
                        column: x => x.currency_id,
                        principalSchema: "public",
                        principalTable: "currencies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_deposit_addresses_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_deposit_addresses_xpubs_xpub_id",
                        column: x => x.xpub_id,
                        principalSchema: "public",
                        principalTable: "xpubs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_deposit_addresses_currency_id",
                schema: "public",
                table: "deposit_addresses",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "IX_deposit_addresses_user_id",
                schema: "public",
                table: "deposit_addresses",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_deposit_addresses_xpub_id",
                schema: "public",
                table: "deposit_addresses",
                column: "xpub_id");

            migrationBuilder.CreateIndex(
                name: "IX_xpubs_currency_id",
                schema: "public",
                table: "xpubs",
                column: "currency_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "deposit_addresses",
                schema: "public");

            migrationBuilder.DropTable(
                name: "variables",
                schema: "public");

            migrationBuilder.DropTable(
                name: "xpubs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "currencies",
                schema: "public");
        }
    }
}
