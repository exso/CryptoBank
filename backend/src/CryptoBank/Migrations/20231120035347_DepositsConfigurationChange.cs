using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CryptoBank.Migrations
{
    /// <inheritdoc />
    public partial class DepositsConfigurationChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_variables",
                schema: "public",
                table: "variables");

            migrationBuilder.DropColumn(
                name: "id",
                schema: "public",
                table: "variables");

            migrationBuilder.AddPrimaryKey(
                name: "PK_variables",
                schema: "public",
                table: "variables",
                column: "key");

            migrationBuilder.CreateIndex(
                name: "IX_variables_key",
                schema: "public",
                table: "variables",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_deposit_addresses_crypto_address",
                schema: "public",
                table: "deposit_addresses",
                column: "crypto_address",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_variables",
                schema: "public",
                table: "variables");

            migrationBuilder.DropIndex(
                name: "IX_variables_key",
                schema: "public",
                table: "variables");

            migrationBuilder.DropIndex(
                name: "IX_deposit_addresses_crypto_address",
                schema: "public",
                table: "deposit_addresses");

            migrationBuilder.AddColumn<int>(
                name: "id",
                schema: "public",
                table: "variables",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_variables",
                schema: "public",
                table: "variables",
                column: "id");
        }
    }
}
