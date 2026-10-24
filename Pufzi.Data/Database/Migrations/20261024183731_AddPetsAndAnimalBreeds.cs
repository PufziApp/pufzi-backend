using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Pufzi.Data.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddPetsAndAnimalBreeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnimalBreeds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnimalSpeciesId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimalBreeds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnimalBreeds_AnimalSpecies_AnimalSpeciesId",
                        column: x => x.AnimalSpeciesId,
                        principalTable: "AnimalSpecies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AnimalSpeciesId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnimalBreedId = table.Column<Guid>(type: "uuid", nullable: true),
                    Sex = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                    WeightKg = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: true),
                    HeightCm = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: true),
                    Color = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MicrochipStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Unknown"),
                    MicrochipNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    SterilizationStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Unknown"),
                    ImportantInformationStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Unknown"),
                    ImportantInformation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ProfileImageBlobName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pets", x => x.Id);
                    table.CheckConstraint("CK_Pets_HeightCm", "\"HeightCm\" IS NULL OR (\"HeightCm\" > 0 AND \"HeightCm\" <= 99999)");
                    table.CheckConstraint("CK_Pets_WeightKg", "\"WeightKg\" IS NULL OR (\"WeightKg\" > 0 AND \"WeightKg\" <= 99999)");
                    table.ForeignKey(
                        name: "FK_Pets_AnimalBreeds_AnimalBreedId",
                        column: x => x.AnimalBreedId,
                        principalTable: "AnimalBreeds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pets_AnimalSpecies_AnimalSpeciesId",
                        column: x => x.AnimalSpeciesId,
                        principalTable: "AnimalSpecies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PetImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PetId = table.Column<Guid>(type: "uuid", nullable: false),
                    BlobName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PetImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PetImages_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AnimalBreeds",
                columns: new[] { "Id", "AnimalSpeciesId", "Code", "IsActive", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-000000000001"), new Guid("11111111-1111-1111-1111-111111111111"), "beagle", true, 1 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-000000000002"), new Guid("11111111-1111-1111-1111-111111111111"), "bichon_frise", true, 2 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-000000000003"), new Guid("11111111-1111-1111-1111-111111111111"), "maltese", true, 3 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-000000000004"), new Guid("11111111-1111-1111-1111-111111111111"), "border_collie", true, 4 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-000000000005"), new Guid("11111111-1111-1111-1111-111111111111"), "french_bulldog", true, 5 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-000000000006"), new Guid("11111111-1111-1111-1111-111111111111"), "german_shepherd", true, 6 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-000000000007"), new Guid("11111111-1111-1111-1111-111111111111"), "golden_retriever", true, 7 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-000000000008"), new Guid("11111111-1111-1111-1111-111111111111"), "labrador_retriever", true, 8 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-000000000009"), new Guid("11111111-1111-1111-1111-111111111111"), "poodle", true, 9 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-000000000010"), new Guid("11111111-1111-1111-1111-111111111111"), "shih_tzu", true, 10 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-000000000011"), new Guid("11111111-1111-1111-1111-111111111111"), "mixed_breed", true, 11 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-000000000101"), new Guid("22222222-2222-2222-2222-222222222222"), "british_shorthair", true, 1 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-000000000102"), new Guid("22222222-2222-2222-2222-222222222222"), "maine_coon", true, 2 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-000000000103"), new Guid("22222222-2222-2222-2222-222222222222"), "persian", true, 3 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-000000000104"), new Guid("22222222-2222-2222-2222-222222222222"), "ragdoll", true, 4 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-000000000105"), new Guid("22222222-2222-2222-2222-222222222222"), "siamese", true, 5 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-000000000106"), new Guid("22222222-2222-2222-2222-222222222222"), "sphynx", true, 6 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-000000000107"), new Guid("22222222-2222-2222-2222-222222222222"), "mixed_breed", true, 7 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnimalBreeds_AnimalSpeciesId_Code",
                table: "AnimalBreeds",
                columns: new[] { "AnimalSpeciesId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PetImages_PetId_SortOrder",
                table: "PetImages",
                columns: new[] { "PetId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Pets_AnimalBreedId",
                table: "Pets",
                column: "AnimalBreedId");

            migrationBuilder.CreateIndex(
                name: "IX_Pets_AnimalSpeciesId",
                table: "Pets",
                column: "AnimalSpeciesId");

            migrationBuilder.CreateIndex(
                name: "IX_Pets_UserId_IsArchived_CreatedAt",
                table: "Pets",
                columns: new[] { "UserId", "IsArchived", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PetImages");

            migrationBuilder.DropTable(
                name: "Pets");

            migrationBuilder.DropTable(
                name: "AnimalBreeds");
        }
    }
}
