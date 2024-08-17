using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Kyyla.Model.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LockEvents",
                columns: table => new
                {
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EventType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LockEvents", x => x.Timestamp);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LockEvents");
        }
    }
}
