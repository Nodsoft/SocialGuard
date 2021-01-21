using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Natsecure.SocialGuard.Api.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrustlistUsers",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    EntryAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastEscalated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EscalationLevel = table.Column<byte>(type: "tinyint", nullable: false),
                    EscalationNote = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrustlistUsers", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrustlistUsers");
        }
    }
}
