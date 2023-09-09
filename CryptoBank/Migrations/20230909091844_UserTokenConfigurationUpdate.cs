using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CryptoBank.Migrations
{
    /// <inheritdoc />
    public partial class UserTokenConfigurationUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "refresh_tokens",
                schema: "public");

            migrationBuilder.CreateTable(
                name: "user_tokens",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    token = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    expires = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revoked = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    replaced_by_token = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    reason_revoked = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_tokens_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_tokens_user_id",
                schema: "public",
                table: "user_tokens",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_tokens",
                schema: "public");

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_ip = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    expires = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_expired = table.Column<bool>(type: "boolean", nullable: false),
                    is_revoked = table.Column<bool>(type: "boolean", nullable: false),
                    reason_revoked = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    replaced_by_token = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    revoked = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoked_by_ip = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    token = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_user_id",
                schema: "public",
                table: "refresh_tokens",
                column: "user_id");
        }
    }
}
