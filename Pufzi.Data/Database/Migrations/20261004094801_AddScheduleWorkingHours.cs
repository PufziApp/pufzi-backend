using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pufzi.Data.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleWorkingHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeWorkingHour_BusinessMemberships_BusinessMembershipId",
                table: "EmployeeWorkingHour");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeWorkingHour",
                table: "EmployeeWorkingHour");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeWorkingHour_BusinessMembershipId",
                table: "EmployeeWorkingHour");

            migrationBuilder.RenameTable(
                name: "EmployeeWorkingHour",
                newName: "EmployeeWorkingHours");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeWorkingHours",
                table: "EmployeeWorkingHours",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "BusinessWorkingHours",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    IsOpen = table.Column<bool>(type: "boolean", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessWorkingHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessWorkingHours_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeWorkingHours_BusinessMembershipId_DayOfWeek",
                table: "EmployeeWorkingHours",
                columns: new[] { "BusinessMembershipId", "DayOfWeek" });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessWorkingHours_BusinessId_DayOfWeek",
                table: "BusinessWorkingHours",
                columns: new[] { "BusinessId", "DayOfWeek" });

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeWorkingHours_BusinessMemberships_BusinessMembership~",
                table: "EmployeeWorkingHours",
                column: "BusinessMembershipId",
                principalTable: "BusinessMemberships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeWorkingHours_BusinessMemberships_BusinessMembership~",
                table: "EmployeeWorkingHours");

            migrationBuilder.DropTable(
                name: "BusinessWorkingHours");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeWorkingHours",
                table: "EmployeeWorkingHours");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeWorkingHours_BusinessMembershipId_DayOfWeek",
                table: "EmployeeWorkingHours");

            migrationBuilder.RenameTable(
                name: "EmployeeWorkingHours",
                newName: "EmployeeWorkingHour");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeWorkingHour",
                table: "EmployeeWorkingHour",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeWorkingHour_BusinessMembershipId",
                table: "EmployeeWorkingHour",
                column: "BusinessMembershipId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeWorkingHour_BusinessMemberships_BusinessMembershipId",
                table: "EmployeeWorkingHour",
                column: "BusinessMembershipId",
                principalTable: "BusinessMemberships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
