using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Pufzi.Data.Database.Migrations
{
    /// <inheritdoc />
    public partial class SeedAnimalSpecies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AnimalSpecies",
                columns: new[] { "Id", "Code", "IsActive", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "dog", true, 1 },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "cat", true, 2 },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "rabbit", true, 3 },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "guinea_pig", true, 4 },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "hamster", true, 5 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AnimalSpecies",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "AnimalSpecies",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "AnimalSpecies",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "AnimalSpecies",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "AnimalSpecies",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"));
        }
    }
}
