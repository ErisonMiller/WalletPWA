using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WalletPWA.Server.Migrations
{
    public partial class PatrimonyTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "patrimonies",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    priceToday = table.Column<float>(type: "real", nullable: false),
                    priceOriginal = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patrimonies", x => new { x.UserId, x.Date });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "patrimonies");
        }
    }
}
