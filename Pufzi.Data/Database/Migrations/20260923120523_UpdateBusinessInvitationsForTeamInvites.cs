using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pufzi.Data.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBusinessInvitationsForTeamInvites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TokenHash",
                table: "BusinessInvitations",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<DateTime>(
                name: "RevokedAt",
                table: "BusinessInvitations",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RevokedAt",
                table: "BusinessInvitations");

            migrationBuilder.AlterColumn<string>(
                name: "TokenHash",
                table: "BusinessInvitations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);
        }
    }
}
