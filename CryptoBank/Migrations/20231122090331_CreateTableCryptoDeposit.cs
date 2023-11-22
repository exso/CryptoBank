using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CryptoBank.Migrations
{
    /// <inheritdoc />
    public partial class CreateTableCryptoDeposit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "crypto_deposits",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    address_id = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(20,2)", precision: 20, scale: 2, nullable: false),
                    currency_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    tx_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    confirmations = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crypto_deposits", x => x.id);
                    table.ForeignKey(
                        name: "FK_crypto_deposits_currencies_currency_id",
                        column: x => x.currency_id,
                        principalSchema: "public",
                        principalTable: "currencies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_crypto_deposits_deposit_addresses_address_id",
                        column: x => x.address_id,
                        principalSchema: "public",
                        principalTable: "deposit_addresses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_crypto_deposits_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_crypto_deposits_address_id",
                schema: "public",
                table: "crypto_deposits",
                column: "address_id");

            migrationBuilder.CreateIndex(
                name: "IX_crypto_deposits_currency_id",
                schema: "public",
                table: "crypto_deposits",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "IX_crypto_deposits_user_id",
                schema: "public",
                table: "crypto_deposits",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "crypto_deposits",
                schema: "public");
        }
    }
}
