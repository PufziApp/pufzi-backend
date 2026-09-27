using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pufzi.Data.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamManagementFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoverImageBlobName",
                table: "BusinessMemberships",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RemovedAt",
                table: "BusinessMemberships",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverImageBlobName",
                table: "BusinessMemberships");

            migrationBuilder.DropColumn(
                name: "RemovedAt",
                table: "BusinessMemberships");
        }
    }
}
