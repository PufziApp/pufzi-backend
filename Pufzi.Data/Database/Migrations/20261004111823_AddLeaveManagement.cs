using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pufzi.Data.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaveManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeLeaveBalance_BusinessMemberships_BusinessMembership~",
                table: "EmployeeLeaveBalance");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequest_BusinessMemberships_BusinessMembershipId",
                table: "LeaveRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequest_Users_ReviewedByUserId",
                table: "LeaveRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LeaveRequest",
                table: "LeaveRequest");

            migrationBuilder.DropIndex(
                name: "IX_LeaveRequest_BusinessMembershipId",
                table: "LeaveRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeLeaveBalance",
                table: "EmployeeLeaveBalance");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeLeaveBalance_BusinessMembershipId",
                table: "EmployeeLeaveBalance");

            migrationBuilder.RenameTable(
                name: "LeaveRequest",
                newName: "LeaveRequests");

            migrationBuilder.RenameTable(
                name: "EmployeeLeaveBalance",
                newName: "EmployeeLeaveBalances");

            migrationBuilder.RenameIndex(
                name: "IX_LeaveRequest_ReviewedByUserId",
                table: "LeaveRequests",
                newName: "IX_LeaveRequests_ReviewedByUserId");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "LeaveRequests",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewNote",
                table: "LeaveRequests",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "LeaveRequests",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "LeaveRequests",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LeaveRequests",
                table: "LeaveRequests",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeLeaveBalances",
                table: "EmployeeLeaveBalances",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "BusinessScheduleExceptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    IsClosed = table.Column<bool>(type: "boolean", nullable: false),
                    OpenTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    CloseTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessScheduleExceptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessScheduleExceptions_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_BusinessMembershipId_StartDate_EndDate",
                table: "LeaveRequests",
                columns: new[] { "BusinessMembershipId", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_BusinessMembershipId_Status",
                table: "LeaveRequests",
                columns: new[] { "BusinessMembershipId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_Status_StartDate",
                table: "LeaveRequests",
                columns: new[] { "Status", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLeaveBalances_BusinessMembershipId_Year",
                table: "EmployeeLeaveBalances",
                columns: new[] { "BusinessMembershipId", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessScheduleExceptions_BusinessId_Date",
                table: "BusinessScheduleExceptions",
                columns: new[] { "BusinessId", "Date" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeLeaveBalances_BusinessMemberships_BusinessMembershi~",
                table: "EmployeeLeaveBalances",
                column: "BusinessMembershipId",
                principalTable: "BusinessMemberships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_BusinessMemberships_BusinessMembershipId",
                table: "LeaveRequests",
                column: "BusinessMembershipId",
                principalTable: "BusinessMemberships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_Users_ReviewedByUserId",
                table: "LeaveRequests",
                column: "ReviewedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeLeaveBalances_BusinessMemberships_BusinessMembershi~",
                table: "EmployeeLeaveBalances");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_BusinessMemberships_BusinessMembershipId",
                table: "LeaveRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_Users_ReviewedByUserId",
                table: "LeaveRequests");

            migrationBuilder.DropTable(
                name: "BusinessScheduleExceptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LeaveRequests",
                table: "LeaveRequests");

            migrationBuilder.DropIndex(
                name: "IX_LeaveRequests_BusinessMembershipId_StartDate_EndDate",
                table: "LeaveRequests");

            migrationBuilder.DropIndex(
                name: "IX_LeaveRequests_BusinessMembershipId_Status",
                table: "LeaveRequests");

            migrationBuilder.DropIndex(
                name: "IX_LeaveRequests_Status_StartDate",
                table: "LeaveRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeLeaveBalances",
                table: "EmployeeLeaveBalances");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeLeaveBalances_BusinessMembershipId_Year",
                table: "EmployeeLeaveBalances");

            migrationBuilder.DropColumn(
                name: "ReviewNote",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "LeaveRequests");

            migrationBuilder.RenameTable(
                name: "LeaveRequests",
                newName: "LeaveRequest");

            migrationBuilder.RenameTable(
                name: "EmployeeLeaveBalances",
                newName: "EmployeeLeaveBalance");

            migrationBuilder.RenameIndex(
                name: "IX_LeaveRequests_ReviewedByUserId",
                table: "LeaveRequest",
                newName: "IX_LeaveRequest_ReviewedByUserId");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "LeaveRequest",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_LeaveRequest",
                table: "LeaveRequest",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeLeaveBalance",
                table: "EmployeeLeaveBalance",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequest_BusinessMembershipId",
                table: "LeaveRequest",
                column: "BusinessMembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLeaveBalance_BusinessMembershipId",
                table: "EmployeeLeaveBalance",
                column: "BusinessMembershipId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeLeaveBalance_BusinessMemberships_BusinessMembership~",
                table: "EmployeeLeaveBalance",
                column: "BusinessMembershipId",
                principalTable: "BusinessMemberships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequest_BusinessMemberships_BusinessMembershipId",
                table: "LeaveRequest",
                column: "BusinessMembershipId",
                principalTable: "BusinessMemberships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequest_Users_ReviewedByUserId",
                table: "LeaveRequest",
                column: "ReviewedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
