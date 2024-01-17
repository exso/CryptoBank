using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoBank.Migrations
{
    /// <inheritdoc />
    public partial class ReplacedByTokenIdForeignKeyToUserToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "replaced_by_token",
                schema: "public",
                table: "user_tokens");

            migrationBuilder.AddColumn<int>(
                name: "replaced_by_token_id",
                schema: "public",
                table: "user_tokens",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_tokens_replaced_by_token_id",
                schema: "public",
                table: "user_tokens",
                column: "replaced_by_token_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_user_tokens_user_tokens_replaced_by_token_id",
                schema: "public",
                table: "user_tokens",
                column: "replaced_by_token_id",
                principalSchema: "public",
                principalTable: "user_tokens",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_tokens_user_tokens_replaced_by_token_id",
                schema: "public",
                table: "user_tokens");

            migrationBuilder.DropIndex(
                name: "IX_user_tokens_replaced_by_token_id",
                schema: "public",
                table: "user_tokens");

            migrationBuilder.DropColumn(
                name: "replaced_by_token_id",
                schema: "public",
                table: "user_tokens");

            migrationBuilder.AddColumn<string>(
                name: "replaced_by_token",
                schema: "public",
                table: "user_tokens",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);
        }
    }
}
