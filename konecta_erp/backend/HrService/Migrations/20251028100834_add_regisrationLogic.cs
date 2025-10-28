using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrService.Migrations
{
    /// <inheritdoc />
    public partial class add_regisrationLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "ResignationRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DecidedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DecisionNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    EligibleForRehire = table.Column<bool>(type: "bit", nullable: true),
                    ApprovedByEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResignationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResignationRequests_Employees_ApprovedByEmployeeId",
                        column: x => x.ApprovedByEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ResignationRequests_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResignationRequests_ApprovedByEmployeeId",
                table: "ResignationRequests",
                column: "ApprovedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ResignationRequests_EmployeeId_Status",
                table: "ResignationRequests",
                columns: new[] { "EmployeeId", "Status" },
                filter: "[Status] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResignationRequests");

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
    }
}
