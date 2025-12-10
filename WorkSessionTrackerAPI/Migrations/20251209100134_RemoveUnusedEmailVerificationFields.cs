using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkSessionTrackerAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedEmailVerificationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailVerificationToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmailVerificationTokenExpiration",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailVerificationToken",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerificationTokenExpiration",
                table: "Users",
                type: "datetime2",
                nullable: true);
        }
    }
}
