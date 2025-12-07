using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkSessionTrackerAPI.Migrations
{
    /// <inheritdoc />
    public partial class EmailVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmailToken",
                table: "Supervisors",
                newName: "EmailVerificationToken");

            migrationBuilder.RenameColumn(
                name: "EmailToken",
                table: "Employees",
                newName: "EmailVerificationToken");

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerificationTokenExpiration",
                table: "Supervisors",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailVerified",
                table: "Supervisors",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerificationTokenExpiration",
                table: "Employees",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailVerified",
                table: "Employees",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailVerificationTokenExpiration",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "EmailVerified",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "EmailVerificationTokenExpiration",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmailVerified",
                table: "Employees");

            migrationBuilder.RenameColumn(
                name: "EmailVerificationToken",
                table: "Supervisors",
                newName: "EmailToken");

            migrationBuilder.RenameColumn(
                name: "EmailVerificationToken",
                table: "Employees",
                newName: "EmailToken");
        }
    }
}
