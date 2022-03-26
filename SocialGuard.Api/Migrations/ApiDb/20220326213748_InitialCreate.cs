using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialGuard.Api.Migrations.ApiDb
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "emitters",
                columns: table => new
                {
                    login = table.Column<string>(type: "text", nullable: false),
                    emitter_type = table.Column<byte>(type: "smallint", nullable: false),
                    snowflake = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_emitters", x => x.login);
                });

            migrationBuilder.CreateTable(
                name: "trustlist_users",
                columns: table => new
                {
                    id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trustlist_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "trustlist_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entry_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_escalated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    escalation_level = table.Column<byte>(type: "smallint", nullable: false),
                    escalation_note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    emitter_login = table.Column<string>(type: "text", nullable: true),
                    trustlist_user_id = table.Column<decimal>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trustlist_entries", x => x.id);
                    table.ForeignKey(
                        name: "fk_trustlist_entries_emitters_emitter_login",
                        column: x => x.emitter_login,
                        principalTable: "emitters",
                        principalColumn: "login");
                    table.ForeignKey(
                        name: "fk_trustlist_entries_trustlist_users_trustlist_user_id",
                        column: x => x.trustlist_user_id,
                        principalTable: "trustlist_users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_trustlist_entries_emitter_login",
                table: "trustlist_entries",
                column: "emitter_login");

            migrationBuilder.CreateIndex(
                name: "ix_trustlist_entries_trustlist_user_id",
                table: "trustlist_entries",
                column: "trustlist_user_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trustlist_entries");

            migrationBuilder.DropTable(
                name: "emitters");

            migrationBuilder.DropTable(
                name: "trustlist_users");
        }
    }
}
