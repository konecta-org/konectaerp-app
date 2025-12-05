using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrService.Migrations
{
    /// <inheritdoc />
    public partial class secondeditedmodelsmigrationv2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EligibleForRehire",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ExitDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ExitReason",
                table: "Employees");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EligibleForRehire",
                table: "Employees",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExitDate",
                table: "Employees",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExitReason",
                table: "Employees",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }
    }
}
