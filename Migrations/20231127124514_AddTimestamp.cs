using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datarouter.Migrations
{
    /// <inheritdoc />
    public partial class AddTimestamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Timestamp",
                table: "Events",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "Events");
        }
    }
}
