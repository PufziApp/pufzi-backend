using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pufzi.Data.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessCountryAndTimeZone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "Businesses",
                type: "character varying(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "RO");

            migrationBuilder.AddColumn<string>(
                name: "TimeZoneId",
                table: "Businesses",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "Europe/Bucharest");

            migrationBuilder.CreateTable(
                name: "LeaveRequestBalanceUsages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Days = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveRequestBalanceUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveRequestBalanceUsages_LeaveRequests_LeaveRequestId",
                        column: x => x.LeaveRequestId,
                        principalTable: "LeaveRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequestBalanceUsages_LeaveRequestId_Year",
                table: "LeaveRequestBalanceUsages",
                columns: new[] { "LeaveRequestId", "Year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeaveRequestBalanceUsages");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "TimeZoneId",
                table: "Businesses");
        }
    }
}
