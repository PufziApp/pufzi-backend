CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE "Businesses" (
    "Id" uuid NOT NULL,
    "Name" character varying(200) NOT NULL,
    "Slug" character varying(200) NOT NULL,
    "Description" character varying(2000),
    "PhoneNumber" character varying(30),
    "ContactEmail" character varying(320),
    "AddressLine" character varying(300),
    "City" character varying(100),
    "County" character varying(100),
    "PostalCode" character varying(20),
    "Latitude" numeric(9,6),
    "Longitude" numeric(9,6),
    "LogoBlobName" character varying(500),
    "CoverImageBlobName" character varying(500),
    "InstagramUrl" character varying(500),
    "FacebookUrl" character varying(500),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Businesses" PRIMARY KEY ("Id")
);

CREATE TABLE "Users" (
    "Id" uuid NOT NULL,
    "FirstName" character varying(100) NOT NULL,
    "LastName" character varying(100) NOT NULL,
    "Email" character varying(320) NOT NULL,
    "NormalizedEmail" character varying(320) NOT NULL,
    "PasswordHash" character varying(500),
    "PlatformRole" character varying(50) NOT NULL,
    "PhoneNumber" character varying(30),
    "ProfileImageBlobName" character varying(500),
    "EmailConfirmed" boolean NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
);

CREATE TABLE "BusinessInvitations" (
    "Id" uuid NOT NULL,
    "BusinessId" uuid NOT NULL,
    "InvitedByUserId" uuid NOT NULL,
    "Email" character varying(320) NOT NULL,
    "Role" character varying(50) NOT NULL,
    "TokenHash" character varying(500) NOT NULL,
    "ExpiresAt" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "AcceptedAt" timestamp with time zone,
    CONSTRAINT "PK_BusinessInvitations" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_BusinessInvitations_Businesses_BusinessId" FOREIGN KEY ("BusinessId") REFERENCES "Businesses" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_BusinessInvitations_Users_InvitedByUserId" FOREIGN KEY ("InvitedByUserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "BusinessMemberships" (
    "Id" uuid NOT NULL,
    "BusinessId" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Role" character varying(50) NOT NULL,
    "JobTitle" character varying(100),
    "Bio" character varying(1000),
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_BusinessMemberships" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_BusinessMemberships_Businesses_BusinessId" FOREIGN KEY ("BusinessId") REFERENCES "Businesses" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_BusinessMemberships_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ExternalLogins" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Provider" character varying(50) NOT NULL,
    "ProviderUserId" character varying(255) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_ExternalLogins" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ExternalLogins_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "RefreshTokens" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "TokenHash" character varying(500) NOT NULL,
    "ExpiresAt" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "RevokedAt" timestamp with time zone,
    "ReplacedByTokenHash" character varying(500),
    CONSTRAINT "PK_RefreshTokens" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_RefreshTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "UserTokens" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Type" character varying(50) NOT NULL,
    "TokenHash" character varying(500) NOT NULL,
    "ExpiresAt" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UsedAt" timestamp with time zone,
    CONSTRAINT "PK_UserTokens" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_UserTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "EmployeeLeaveBalance" (
    "Id" uuid NOT NULL,
    "BusinessMembershipId" uuid NOT NULL,
    "Year" integer NOT NULL,
    "TotalDays" integer NOT NULL,
    "UsedDays" integer NOT NULL,
    CONSTRAINT "PK_EmployeeLeaveBalance" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_EmployeeLeaveBalance_BusinessMemberships_BusinessMembership~" FOREIGN KEY ("BusinessMembershipId") REFERENCES "BusinessMemberships" ("Id") ON DELETE CASCADE
);

CREATE TABLE "EmployeeWorkingHour" (
    "Id" uuid NOT NULL,
    "BusinessMembershipId" uuid NOT NULL,
    "DayOfWeek" integer NOT NULL,
    "IsWorking" boolean NOT NULL,
    "StartTime" time without time zone,
    "EndTime" time without time zone,
    CONSTRAINT "PK_EmployeeWorkingHour" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_EmployeeWorkingHour_BusinessMemberships_BusinessMembershipId" FOREIGN KEY ("BusinessMembershipId") REFERENCES "BusinessMemberships" ("Id") ON DELETE CASCADE
);

CREATE TABLE "LeaveRequest" (
    "Id" uuid NOT NULL,
    "BusinessMembershipId" uuid NOT NULL,
    "StartDate" date NOT NULL,
    "EndDate" date NOT NULL,
    "RequestedDays" integer NOT NULL,
    "Reason" text,
    "Status" integer NOT NULL,
    "ReviewedByUserId" uuid,
    "ReviewedAt" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_LeaveRequest" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_LeaveRequest_BusinessMemberships_BusinessMembershipId" FOREIGN KEY ("BusinessMembershipId") REFERENCES "BusinessMemberships" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_LeaveRequest_Users_ReviewedByUserId" FOREIGN KEY ("ReviewedByUserId") REFERENCES "Users" ("Id")
);

CREATE UNIQUE INDEX "IX_Businesses_Slug" ON "Businesses" ("Slug");

CREATE INDEX "IX_BusinessInvitations_BusinessId_Email" ON "BusinessInvitations" ("BusinessId", "Email");

CREATE INDEX "IX_BusinessInvitations_InvitedByUserId" ON "BusinessInvitations" ("InvitedByUserId");

CREATE UNIQUE INDEX "IX_BusinessInvitations_TokenHash" ON "BusinessInvitations" ("TokenHash");

CREATE UNIQUE INDEX "IX_BusinessMemberships_BusinessId_UserId" ON "BusinessMemberships" ("BusinessId", "UserId");

CREATE INDEX "IX_BusinessMemberships_UserId" ON "BusinessMemberships" ("UserId");

CREATE INDEX "IX_EmployeeLeaveBalance_BusinessMembershipId" ON "EmployeeLeaveBalance" ("BusinessMembershipId");

CREATE INDEX "IX_EmployeeWorkingHour_BusinessMembershipId" ON "EmployeeWorkingHour" ("BusinessMembershipId");

CREATE UNIQUE INDEX "IX_ExternalLogins_Provider_ProviderUserId" ON "ExternalLogins" ("Provider", "ProviderUserId");

CREATE INDEX "IX_ExternalLogins_UserId" ON "ExternalLogins" ("UserId");

CREATE INDEX "IX_LeaveRequest_BusinessMembershipId" ON "LeaveRequest" ("BusinessMembershipId");

CREATE INDEX "IX_LeaveRequest_ReviewedByUserId" ON "LeaveRequest" ("ReviewedByUserId");

CREATE UNIQUE INDEX "IX_RefreshTokens_TokenHash" ON "RefreshTokens" ("TokenHash");

CREATE INDEX "IX_RefreshTokens_UserId" ON "RefreshTokens" ("UserId");

CREATE UNIQUE INDEX "IX_Users_NormalizedEmail" ON "Users" ("NormalizedEmail");

CREATE UNIQUE INDEX "IX_UserTokens_TokenHash" ON "UserTokens" ("TokenHash");

CREATE INDEX "IX_UserTokens_UserId_Type" ON "UserTokens" ("UserId", "Type");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260907192628_InitialCreate', '10.0.4');

COMMIT;

START TRANSACTION;
CREATE TABLE "BusinessImages" (
    "Id" uuid NOT NULL,
    "BusinessId" uuid NOT NULL,
    "BlobName" text NOT NULL,
    "SortOrder" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_BusinessImages" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_BusinessImages_Businesses_BusinessId" FOREIGN KEY ("BusinessId") REFERENCES "Businesses" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_BusinessImages_BusinessId" ON "BusinessImages" ("BusinessId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260909202616_AddBusinessImages', '10.0.4');

COMMIT;

START TRANSACTION;
CREATE TABLE "AnimalSpecies" (
    "Id" uuid NOT NULL,
    "Code" character varying(50) NOT NULL,
    "IsActive" boolean NOT NULL,
    "SortOrder" integer NOT NULL,
    CONSTRAINT "PK_AnimalSpecies" PRIMARY KEY ("Id")
);

CREATE TABLE "Services" (
    "Id" uuid NOT NULL,
    "BusinessId" uuid NOT NULL,
    "Name" character varying(150) NOT NULL,
    "Description" character varying(2000),
    "ImageBlobName" character varying(500),
    "IsActive" boolean NOT NULL,
    "SortOrder" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Services" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Services_Businesses_BusinessId" FOREIGN KEY ("BusinessId") REFERENCES "Businesses" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ServiceOptions" (
    "Id" uuid NOT NULL,
    "ServiceId" uuid NOT NULL,
    "AnimalSpeciesId" uuid NOT NULL,
    "DurationMinutes" integer,
    "SortOrder" integer NOT NULL,
    CONSTRAINT "PK_ServiceOptions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ServiceOptions_AnimalSpecies_AnimalSpeciesId" FOREIGN KEY ("AnimalSpeciesId") REFERENCES "AnimalSpecies" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_ServiceOptions_Services_ServiceId" FOREIGN KEY ("ServiceId") REFERENCES "Services" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ServicePriceVariants" (
    "Id" uuid NOT NULL,
    "ServiceOptionId" uuid NOT NULL,
    "Name" character varying(100) NOT NULL,
    "MinWeightKg" numeric(6,2),
    "MaxWeightKg" numeric(6,2),
    "Price" numeric(10,2),
    "IsStartingPrice" boolean NOT NULL,
    "IsPriceOnRequest" boolean NOT NULL,
    "DurationMinutes" integer,
    "SortOrder" integer NOT NULL,
    CONSTRAINT "PK_ServicePriceVariants" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ServicePriceVariants_ServiceOptions_ServiceOptionId" FOREIGN KEY ("ServiceOptionId") REFERENCES "ServiceOptions" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_AnimalSpecies_Code" ON "AnimalSpecies" ("Code");

CREATE INDEX "IX_ServiceOptions_AnimalSpeciesId" ON "ServiceOptions" ("AnimalSpeciesId");

CREATE UNIQUE INDEX "IX_ServiceOptions_ServiceId_AnimalSpeciesId" ON "ServiceOptions" ("ServiceId", "AnimalSpeciesId");

CREATE INDEX "IX_ServicePriceVariants_ServiceOptionId" ON "ServicePriceVariants" ("ServiceOptionId");

CREATE INDEX "IX_Services_BusinessId" ON "Services" ("BusinessId");

CREATE INDEX "IX_Services_BusinessId_IsActive_SortOrder" ON "Services" ("BusinessId", "IsActive", "SortOrder");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260921192341_AddServicesAndAnimalSpecies', '10.0.4');

COMMIT;

START TRANSACTION;
INSERT INTO "AnimalSpecies" ("Id", "Code", "IsActive", "SortOrder")
VALUES ('11111111-1111-1111-1111-111111111111', 'dog', TRUE, 1);
INSERT INTO "AnimalSpecies" ("Id", "Code", "IsActive", "SortOrder")
VALUES ('22222222-2222-2222-2222-222222222222', 'cat', TRUE, 2);
INSERT INTO "AnimalSpecies" ("Id", "Code", "IsActive", "SortOrder")
VALUES ('33333333-3333-3333-3333-333333333333', 'rabbit', TRUE, 3);
INSERT INTO "AnimalSpecies" ("Id", "Code", "IsActive", "SortOrder")
VALUES ('44444444-4444-4444-4444-444444444444', 'guinea_pig', TRUE, 4);
INSERT INTO "AnimalSpecies" ("Id", "Code", "IsActive", "SortOrder")
VALUES ('55555555-5555-5555-5555-555555555555', 'hamster', TRUE, 5);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260921200828_SeedAnimalSpecies', '10.0.4');

COMMIT;

START TRANSACTION;
ALTER TABLE "BusinessInvitations" ALTER COLUMN "TokenHash" TYPE character varying(64);

ALTER TABLE "BusinessInvitations" ADD "RevokedAt" timestamp with time zone;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260923120523_UpdateBusinessInvitationsForTeamInvites', '10.0.4');

COMMIT;

START TRANSACTION;
ALTER TABLE "RefreshTokens" ADD "RememberMe" boolean NOT NULL DEFAULT FALSE;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260926145057_AddRememberMeToRefreshTokens', '10.0.4');

COMMIT;

START TRANSACTION;
ALTER TABLE "BusinessMemberships" ADD "CoverImageBlobName" character varying(500);

ALTER TABLE "BusinessMemberships" ADD "RemovedAt" timestamp with time zone;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260927141451_AddTeamManagementFields', '10.0.4');

COMMIT;

START TRANSACTION;
DROP TABLE "ServicePriceVariants";

DROP TABLE "ServiceOptions";

DROP TABLE "Services";

CREATE TABLE "ServicePackages" (
    "Id" uuid NOT NULL,
    "BusinessId" uuid NOT NULL,
    "Name" character varying(150) NOT NULL,
    "Description" character varying(2000),
    "Icon" character varying(50),
    "IsActive" boolean NOT NULL,
    "SortOrder" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_ServicePackages" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ServicePackages_Businesses_BusinessId" FOREIGN KEY ("BusinessId") REFERENCES "Businesses" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ServicePackageAnimalSpecies" (
    "ServicePackageId" uuid NOT NULL,
    "AnimalSpeciesId" uuid NOT NULL,
    CONSTRAINT "PK_ServicePackageAnimalSpecies" PRIMARY KEY ("ServicePackageId", "AnimalSpeciesId"),
    CONSTRAINT "FK_ServicePackageAnimalSpecies_AnimalSpecies_AnimalSpeciesId" FOREIGN KEY ("AnimalSpeciesId") REFERENCES "AnimalSpecies" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_ServicePackageAnimalSpecies_ServicePackages_ServicePackageId" FOREIGN KEY ("ServicePackageId") REFERENCES "ServicePackages" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ServicePackageEmployees" (
    "ServicePackageId" uuid NOT NULL,
    "BusinessMembershipId" uuid NOT NULL,
    CONSTRAINT "PK_ServicePackageEmployees" PRIMARY KEY ("ServicePackageId", "BusinessMembershipId"),
    CONSTRAINT "FK_ServicePackageEmployees_BusinessMemberships_BusinessMembers~" FOREIGN KEY ("BusinessMembershipId") REFERENCES "BusinessMemberships" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ServicePackageEmployees_ServicePackages_ServicePackageId" FOREIGN KEY ("ServicePackageId") REFERENCES "ServicePackages" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ServicePackageItems" (
    "Id" uuid NOT NULL,
    "ServicePackageId" uuid NOT NULL,
    "Name" character varying(150) NOT NULL,
    "SortOrder" integer NOT NULL,
    CONSTRAINT "PK_ServicePackageItems" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ServicePackageItems_ServicePackages_ServicePackageId" FOREIGN KEY ("ServicePackageId") REFERENCES "ServicePackages" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ServicePackageItemOptions" (
    "Id" uuid NOT NULL,
    "ServicePackageItemId" uuid NOT NULL,
    "AnimalSpeciesId" uuid NOT NULL,
    "SortOrder" integer NOT NULL,
    CONSTRAINT "PK_ServicePackageItemOptions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ServicePackageItemOptions_AnimalSpecies_AnimalSpeciesId" FOREIGN KEY ("AnimalSpeciesId") REFERENCES "AnimalSpecies" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_ServicePackageItemOptions_ServicePackageItems_ServicePackag~" FOREIGN KEY ("ServicePackageItemId") REFERENCES "ServicePackageItems" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ServicePackageItemVariants" (
    "Id" uuid NOT NULL,
    "ServicePackageItemOptionId" uuid NOT NULL,
    "Name" character varying(100) NOT NULL,
    "MinWeightKg" numeric(8,2),
    "MaxWeightKg" numeric(8,2),
    "Price" numeric(10,2),
    "IsStartingPrice" boolean NOT NULL,
    "IsPriceOnRequest" boolean NOT NULL,
    "DurationMinutes" integer,
    "SortOrder" integer NOT NULL,
    CONSTRAINT "PK_ServicePackageItemVariants" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ServicePackageItemVariants_ServicePackageItemOptions_Servic~" FOREIGN KEY ("ServicePackageItemOptionId") REFERENCES "ServicePackageItemOptions" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_ServicePackageAnimalSpecies_AnimalSpeciesId" ON "ServicePackageAnimalSpecies" ("AnimalSpeciesId");

CREATE INDEX "IX_ServicePackageEmployees_BusinessMembershipId" ON "ServicePackageEmployees" ("BusinessMembershipId");

CREATE INDEX "IX_ServicePackageItemOptions_AnimalSpeciesId" ON "ServicePackageItemOptions" ("AnimalSpeciesId");

CREATE UNIQUE INDEX "IX_ServicePackageItemOptions_ServicePackageItemId_AnimalSpecie~" ON "ServicePackageItemOptions" ("ServicePackageItemId", "AnimalSpeciesId");

CREATE INDEX "IX_ServicePackageItems_ServicePackageId" ON "ServicePackageItems" ("ServicePackageId");

CREATE INDEX "IX_ServicePackageItemVariants_ServicePackageItemOptionId" ON "ServicePackageItemVariants" ("ServicePackageItemOptionId");

CREATE INDEX "IX_ServicePackages_BusinessId" ON "ServicePackages" ("BusinessId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260927193709_ReplaceServicesWithServicePackages', '10.0.4');

COMMIT;

START TRANSACTION;
ALTER TABLE "EmployeeWorkingHour" DROP CONSTRAINT "FK_EmployeeWorkingHour_BusinessMemberships_BusinessMembershipId";

ALTER TABLE "EmployeeWorkingHour" DROP CONSTRAINT "PK_EmployeeWorkingHour";

DROP INDEX "IX_EmployeeWorkingHour_BusinessMembershipId";

ALTER TABLE "EmployeeWorkingHour" RENAME TO "EmployeeWorkingHours";

ALTER TABLE "EmployeeWorkingHours" ADD CONSTRAINT "PK_EmployeeWorkingHours" PRIMARY KEY ("Id");

CREATE TABLE "BusinessWorkingHours" (
    "Id" uuid NOT NULL,
    "BusinessId" uuid NOT NULL,
    "DayOfWeek" integer NOT NULL,
    "IsOpen" boolean NOT NULL,
    "StartTime" time without time zone,
    "EndTime" time without time zone,
    CONSTRAINT "PK_BusinessWorkingHours" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_BusinessWorkingHours_Businesses_BusinessId" FOREIGN KEY ("BusinessId") REFERENCES "Businesses" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_EmployeeWorkingHours_BusinessMembershipId_DayOfWeek" ON "EmployeeWorkingHours" ("BusinessMembershipId", "DayOfWeek");

CREATE INDEX "IX_BusinessWorkingHours_BusinessId_DayOfWeek" ON "BusinessWorkingHours" ("BusinessId", "DayOfWeek");

ALTER TABLE "EmployeeWorkingHours" ADD CONSTRAINT "FK_EmployeeWorkingHours_BusinessMemberships_BusinessMembership~" FOREIGN KEY ("BusinessMembershipId") REFERENCES "BusinessMemberships" ("Id") ON DELETE CASCADE;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261004094801_AddScheduleWorkingHours', '10.0.4');

COMMIT;

START TRANSACTION;
ALTER TABLE "EmployeeLeaveBalance" DROP CONSTRAINT "FK_EmployeeLeaveBalance_BusinessMemberships_BusinessMembership~";

ALTER TABLE "LeaveRequest" DROP CONSTRAINT "FK_LeaveRequest_BusinessMemberships_BusinessMembershipId";

ALTER TABLE "LeaveRequest" DROP CONSTRAINT "FK_LeaveRequest_Users_ReviewedByUserId";

ALTER TABLE "LeaveRequest" DROP CONSTRAINT "PK_LeaveRequest";

DROP INDEX "IX_LeaveRequest_BusinessMembershipId";

ALTER TABLE "EmployeeLeaveBalance" DROP CONSTRAINT "PK_EmployeeLeaveBalance";

DROP INDEX "IX_EmployeeLeaveBalance_BusinessMembershipId";

ALTER TABLE "LeaveRequest" RENAME TO "LeaveRequests";

ALTER TABLE "EmployeeLeaveBalance" RENAME TO "EmployeeLeaveBalances";

ALTER INDEX "IX_LeaveRequest_ReviewedByUserId" RENAME TO "IX_LeaveRequests_ReviewedByUserId";

ALTER TABLE "LeaveRequests" ALTER COLUMN "Reason" TYPE character varying(1000);

ALTER TABLE "LeaveRequests" ADD "ReviewNote" character varying(1000);

ALTER TABLE "LeaveRequests" ADD "Type" integer NOT NULL DEFAULT 1;

ALTER TABLE "LeaveRequests" ADD "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP);

ALTER TABLE "LeaveRequests" ADD CONSTRAINT "PK_LeaveRequests" PRIMARY KEY ("Id");

ALTER TABLE "EmployeeLeaveBalances" ADD CONSTRAINT "PK_EmployeeLeaveBalances" PRIMARY KEY ("Id");

CREATE TABLE "BusinessScheduleExceptions" (
    "Id" uuid NOT NULL,
    "BusinessId" uuid NOT NULL,
    "Date" date NOT NULL,
    "IsClosed" boolean NOT NULL,
    "OpenTime" time without time zone,
    "CloseTime" time without time zone,
    "Reason" character varying(500),
    CONSTRAINT "PK_BusinessScheduleExceptions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_BusinessScheduleExceptions_Businesses_BusinessId" FOREIGN KEY ("BusinessId") REFERENCES "Businesses" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_LeaveRequests_BusinessMembershipId_StartDate_EndDate" ON "LeaveRequests" ("BusinessMembershipId", "StartDate", "EndDate");

CREATE INDEX "IX_LeaveRequests_BusinessMembershipId_Status" ON "LeaveRequests" ("BusinessMembershipId", "Status");

CREATE INDEX "IX_LeaveRequests_Status_StartDate" ON "LeaveRequests" ("Status", "StartDate");

CREATE UNIQUE INDEX "IX_EmployeeLeaveBalances_BusinessMembershipId_Year" ON "EmployeeLeaveBalances" ("BusinessMembershipId", "Year");

CREATE UNIQUE INDEX "IX_BusinessScheduleExceptions_BusinessId_Date" ON "BusinessScheduleExceptions" ("BusinessId", "Date");

ALTER TABLE "EmployeeLeaveBalances" ADD CONSTRAINT "FK_EmployeeLeaveBalances_BusinessMemberships_BusinessMembershi~" FOREIGN KEY ("BusinessMembershipId") REFERENCES "BusinessMemberships" ("Id") ON DELETE CASCADE;

ALTER TABLE "LeaveRequests" ADD CONSTRAINT "FK_LeaveRequests_BusinessMemberships_BusinessMembershipId" FOREIGN KEY ("BusinessMembershipId") REFERENCES "BusinessMemberships" ("Id") ON DELETE CASCADE;

ALTER TABLE "LeaveRequests" ADD CONSTRAINT "FK_LeaveRequests_Users_ReviewedByUserId" FOREIGN KEY ("ReviewedByUserId") REFERENCES "Users" ("Id") ON DELETE SET NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261004111823_AddLeaveManagement', '10.0.4');

COMMIT;

START TRANSACTION;
ALTER TABLE "Businesses" ADD "CountryCode" character varying(2) NOT NULL DEFAULT 'RO';

ALTER TABLE "Businesses" ADD "TimeZoneId" character varying(100) NOT NULL DEFAULT 'Europe/Bucharest';

CREATE TABLE "LeaveRequestBalanceUsages" (
    "Id" uuid NOT NULL,
    "LeaveRequestId" uuid NOT NULL,
    "Year" integer NOT NULL,
    "Days" integer NOT NULL,
    CONSTRAINT "PK_LeaveRequestBalanceUsages" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_LeaveRequestBalanceUsages_LeaveRequests_LeaveRequestId" FOREIGN KEY ("LeaveRequestId") REFERENCES "LeaveRequests" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_LeaveRequestBalanceUsages_LeaveRequestId_Year" ON "LeaveRequestBalanceUsages" ("LeaveRequestId", "Year");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261004125435_AddBusinessCountryAndTimeZone', '10.0.4');

COMMIT;

START TRANSACTION;
CREATE TABLE "AnimalBreeds" (
    "Id" uuid NOT NULL,
    "AnimalSpeciesId" uuid NOT NULL,
    "Code" character varying(100) NOT NULL,
    "IsActive" boolean NOT NULL,
    "SortOrder" integer NOT NULL,
    CONSTRAINT "PK_AnimalBreeds" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AnimalBreeds_AnimalSpecies_AnimalSpeciesId" FOREIGN KEY ("AnimalSpeciesId") REFERENCES "AnimalSpecies" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "Pets" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Name" character varying(100) NOT NULL,
    "AnimalSpeciesId" uuid NOT NULL,
    "AnimalBreedId" uuid,
    "Sex" character varying(30),
    "BirthDate" date,
    "WeightKg" numeric(7,2),
    "HeightCm" numeric(7,2),
    "Color" character varying(100),
    "MicrochipStatus" character varying(30) NOT NULL DEFAULT 'Unknown',
    "MicrochipNumber" character varying(30),
    "SterilizationStatus" character varying(30) NOT NULL DEFAULT 'Unknown',
    "ImportantInformationStatus" character varying(30) NOT NULL DEFAULT 'Unknown',
    "ImportantInformation" character varying(2000),
    "ProfileImageBlobName" character varying(500),
    "IsArchived" boolean NOT NULL DEFAULT FALSE,
    "ArchivedAt" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Pets" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_Pets_HeightCm" CHECK ("HeightCm" IS NULL OR ("HeightCm" > 0 AND "HeightCm" <= 99999)),
    CONSTRAINT "CK_Pets_WeightKg" CHECK ("WeightKg" IS NULL OR ("WeightKg" > 0 AND "WeightKg" <= 99999)),
    CONSTRAINT "FK_Pets_AnimalBreeds_AnimalBreedId" FOREIGN KEY ("AnimalBreedId") REFERENCES "AnimalBreeds" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Pets_AnimalSpecies_AnimalSpeciesId" FOREIGN KEY ("AnimalSpeciesId") REFERENCES "AnimalSpecies" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Pets_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "PetImages" (
    "Id" uuid NOT NULL,
    "PetId" uuid NOT NULL,
    "BlobName" character varying(500) NOT NULL,
    "SortOrder" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_PetImages" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_PetImages_Pets_PetId" FOREIGN KEY ("PetId") REFERENCES "Pets" ("Id") ON DELETE CASCADE
);

INSERT INTO "AnimalBreeds" ("Id", "AnimalSpeciesId", "Code", "IsActive", "SortOrder")
VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-000000000001', '11111111-1111-1111-1111-111111111111', 'beagle', TRUE, 1);
INSERT INTO "AnimalBreeds" ("Id", "AnimalSpeciesId", "Code", "IsActive", "SortOrder")
VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-000000000002', '11111111-1111-1111-1111-111111111111', 'bichon_frise', TRUE, 2);
INSERT INTO "AnimalBreeds" ("Id", "AnimalSpeciesId", "Code", "IsActive", "SortOrder")
VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-000000000003', '11111111-1111-1111-1111-111111111111', 'maltese', TRUE, 3);
INSERT INTO "AnimalBreeds" ("Id", "AnimalSpeciesId", "Code", "IsActive", "SortOrder")
VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-000000000004', '11111111-1111-1111-1111-111111111111', 'border_collie', TRUE, 4);
INSERT INTO "AnimalBreeds" ("Id", "AnimalSpeciesId", "Code", "IsActive", "SortOrder")
VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-000000000005', '11111111-1111-1111-1111-111111111111', 'french_bulldog', TRUE, 5);
INSERT INTO "AnimalBreeds" ("Id", "AnimalSpeciesId", "Code", "IsActive", "SortOrder")
VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-000000000006', '11111111-1111-1111-1111-111111111111', 'german_shepherd', TRUE, 6);
INSERT INTO "AnimalBreeds" ("Id", "AnimalSpeciesId", "Code", "IsActive", "SortOrder")
VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-000000000007', '11111111-1111-1111-1111-111111111111', 'golden_retriever', TRUE, 7);
INSERT INTO "AnimalBreeds" ("Id", "AnimalSpeciesId", "Code", "IsActive", "SortOrder")
VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-000000000008', '11111111-1111-1111-1111-111111111111', 'labrador_retriever', TRUE, 8);
INSERT INTO "AnimalBreeds" ("Id", "AnimalSpeciesId", "Code", "IsActive", "SortOrder")
VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-000000000009', '11111111-1111-1111-1111-111111111111', 'poodle', TRUE, 9);
INSERT INTO "AnimalBreeds" ("Id", "AnimalSpeciesId", "Code", "IsActive", "SortOrder")
VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-000000000010', '11111111-1111-1111-1111-111111111111', 'shih_tzu', TRUE, 10);
INSERT INTO "AnimalBreeds" ("Id", "AnimalSpeciesId", "Code", "IsActive", "SortOrder")
VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-000000000011', '11111111-1111-1111-1111-111111111111', 'mixed_breed', TRUE, 11);
INSERT INTO "AnimalBreeds" ("Id", "AnimalSpeciesId", "Code", "IsActive", "SortOrder")
VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-000000000101', '22222222-2222-2222-2222-222222222222', 'british_shorthair', TRUE, 1);
INSERT INTO "AnimalBreeds" ("Id", "AnimalSpeciesId", "Code", "IsActive", "SortOrder")
VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-000000000102', '22222222-2222-2222-2222-222222222222', 'maine_coon', TRUE, 2);
INSERT INTO "AnimalBreeds" ("Id", "AnimalSpeciesId", "Code", "IsActive", "SortOrder")
VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-000000000103', '22222222-2222-2222-2222-222222222222', 'persian', TRUE, 3);
INSERT INTO "AnimalBreeds" ("Id", "AnimalSpeciesId", "Code", "IsActive", "SortOrder")
VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-000000000104', '22222222-2222-2222-2222-222222222222', 'ragdoll', TRUE, 4);
INSERT INTO "AnimalBreeds" ("Id", "AnimalSpeciesId", "Code", "IsActive", "SortOrder")
VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-000000000105', '22222222-2222-2222-2222-222222222222', 'siamese', TRUE, 5);
INSERT INTO "AnimalBreeds" ("Id", "AnimalSpeciesId", "Code", "IsActive", "SortOrder")
VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-000000000106', '22222222-2222-2222-2222-222222222222', 'sphynx', TRUE, 6);
INSERT INTO "AnimalBreeds" ("Id", "AnimalSpeciesId", "Code", "IsActive", "SortOrder")
VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-000000000107', '22222222-2222-2222-2222-222222222222', 'mixed_breed', TRUE, 7);

CREATE UNIQUE INDEX "IX_AnimalBreeds_AnimalSpeciesId_Code" ON "AnimalBreeds" ("AnimalSpeciesId", "Code");

CREATE INDEX "IX_PetImages_PetId_SortOrder" ON "PetImages" ("PetId", "SortOrder");

CREATE INDEX "IX_Pets_AnimalBreedId" ON "Pets" ("AnimalBreedId");

CREATE INDEX "IX_Pets_AnimalSpeciesId" ON "Pets" ("AnimalSpeciesId");

CREATE INDEX "IX_Pets_UserId_IsArchived_CreatedAt" ON "Pets" ("UserId", "IsArchived", "CreatedAt");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261024183731_AddPetsAndAnimalBreeds', '10.0.4');

COMMIT;

START TRANSACTION;
ALTER TABLE "Businesses" ADD "IsPublished" boolean NOT NULL DEFAULT FALSE;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261024201956_AddBusinessPublishing', '10.0.4');

COMMIT;

START TRANSACTION;
CREATE TABLE "Bookings" (
    "Id" uuid NOT NULL,
    "BusinessId" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "BusinessMembershipId" uuid NOT NULL,
    "Status" integer NOT NULL,
    "Notes" character varying(2000),
    "ExpiresAt" timestamp with time zone,
    "ReviewedByUserId" uuid,
    "ReviewedAt" timestamp with time zone,
    "RejectionReason" character varying(1000),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Bookings" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Bookings_BusinessMemberships_BusinessMembershipId" FOREIGN KEY ("BusinessMembershipId") REFERENCES "BusinessMemberships" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Bookings_Businesses_BusinessId" FOREIGN KEY ("BusinessId") REFERENCES "Businesses" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Bookings_Users_ReviewedByUserId" FOREIGN KEY ("ReviewedByUserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Bookings_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "Appointments" (
    "Id" uuid NOT NULL,
    "BookingId" uuid NOT NULL,
    "PetId" uuid NOT NULL,
    "StartAt" timestamp with time zone NOT NULL,
    "EndAt" timestamp with time zone NOT NULL,
    "DurationMinutes" integer NOT NULL,
    "TotalPrice" numeric(12,2),
    "HasPriceOnRequest" boolean NOT NULL,
    "SortOrder" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Appointments" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_Appointments_PositiveDuration" CHECK ("DurationMinutes" > 0),
    CONSTRAINT "CK_Appointments_ValidTimeRange" CHECK ("EndAt" > "StartAt"),
    CONSTRAINT "FK_Appointments_Bookings_BookingId" FOREIGN KEY ("BookingId") REFERENCES "Bookings" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Appointments_Pets_PetId" FOREIGN KEY ("PetId") REFERENCES "Pets" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "BookingChangeRequests" (
    "Id" uuid NOT NULL,
    "BookingId" uuid NOT NULL,
    "RequestedByUserId" uuid NOT NULL,
    "Status" integer NOT NULL,
    "Reason" character varying(2000),
    "ExpiresAt" timestamp with time zone,
    "ReviewedByUserId" uuid,
    "ReviewedAt" timestamp with time zone,
    "ReviewNote" character varying(1000),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_BookingChangeRequests" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_BookingChangeRequests_Bookings_BookingId" FOREIGN KEY ("BookingId") REFERENCES "Bookings" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_BookingChangeRequests_Users_RequestedByUserId" FOREIGN KEY ("RequestedByUserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_BookingChangeRequests_Users_ReviewedByUserId" FOREIGN KEY ("ReviewedByUserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "BookingHistories" (
    "Id" uuid NOT NULL,
    "BookingId" uuid NOT NULL,
    "PerformedByUserId" uuid,
    "Action" integer NOT NULL,
    "PreviousStatus" integer,
    "NewStatus" integer,
    "Description" character varying(2000),
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_BookingHistories" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_BookingHistories_Bookings_BookingId" FOREIGN KEY ("BookingId") REFERENCES "Bookings" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_BookingHistories_Users_PerformedByUserId" FOREIGN KEY ("PerformedByUserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "AppointmentServices" (
    "Id" uuid NOT NULL,
    "AppointmentId" uuid NOT NULL,
    "ServicePackageId" uuid NOT NULL,
    "PackageName" character varying(200) NOT NULL,
    "DurationMinutes" integer NOT NULL,
    "TotalPrice" numeric(12,2),
    "HasPriceOnRequest" boolean NOT NULL,
    "SortOrder" integer NOT NULL,
    CONSTRAINT "PK_AppointmentServices" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AppointmentServices_Appointments_AppointmentId" FOREIGN KEY ("AppointmentId") REFERENCES "Appointments" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AppointmentServices_ServicePackages_ServicePackageId" FOREIGN KEY ("ServicePackageId") REFERENCES "ServicePackages" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "AppointmentSlots" (
    "Id" uuid NOT NULL,
    "AppointmentId" uuid NOT NULL,
    "BusinessMembershipId" uuid NOT NULL,
    "StartAt" timestamp with time zone NOT NULL,
    "EndAt" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_AppointmentSlots" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_AppointmentSlots_ValidTimeRange" CHECK ("EndAt" > "StartAt"),
    CONSTRAINT "FK_AppointmentSlots_Appointments_AppointmentId" FOREIGN KEY ("AppointmentId") REFERENCES "Appointments" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AppointmentSlots_BusinessMemberships_BusinessMembershipId" FOREIGN KEY ("BusinessMembershipId") REFERENCES "BusinessMemberships" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "BookingChangeAppointments" (
    "Id" uuid NOT NULL,
    "BookingChangeRequestId" uuid NOT NULL,
    "AppointmentId" uuid NOT NULL,
    "ProposedStartAt" timestamp with time zone NOT NULL,
    "ProposedEndAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_BookingChangeAppointments" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_BookingChangeAppointments_ValidTimeRange" CHECK ("ProposedEndAt" > "ProposedStartAt"),
    CONSTRAINT "FK_BookingChangeAppointments_Appointments_AppointmentId" FOREIGN KEY ("AppointmentId") REFERENCES "Appointments" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_BookingChangeAppointments_BookingChangeRequests_BookingChan~" FOREIGN KEY ("BookingChangeRequestId") REFERENCES "BookingChangeRequests" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AppointmentServiceVariants" (
    "Id" uuid NOT NULL,
    "AppointmentServiceId" uuid NOT NULL,
    "ServicePackageItemVariantId" uuid NOT NULL,
    "ItemName" character varying(200) NOT NULL,
    "VariantName" character varying(200) NOT NULL,
    "Price" numeric(12,2),
    "IsStartingPrice" boolean NOT NULL,
    "IsPriceOnRequest" boolean NOT NULL,
    "DurationMinutes" integer NOT NULL,
    "SortOrder" integer NOT NULL,
    CONSTRAINT "PK_AppointmentServiceVariants" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AppointmentServiceVariants_AppointmentServices_AppointmentS~" FOREIGN KEY ("AppointmentServiceId") REFERENCES "AppointmentServices" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AppointmentServiceVariants_ServicePackageItemVariants_Servi~" FOREIGN KEY ("ServicePackageItemVariantId") REFERENCES "ServicePackageItemVariants" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_Appointments_BookingId_StartAt" ON "Appointments" ("BookingId", "StartAt");

CREATE INDEX "IX_Appointments_PetId_StartAt" ON "Appointments" ("PetId", "StartAt");

CREATE INDEX "IX_Appointments_StartAt_EndAt" ON "Appointments" ("StartAt", "EndAt");

CREATE INDEX "IX_AppointmentServices_AppointmentId" ON "AppointmentServices" ("AppointmentId");

CREATE INDEX "IX_AppointmentServices_ServicePackageId" ON "AppointmentServices" ("ServicePackageId");

CREATE INDEX "IX_AppointmentServiceVariants_AppointmentServiceId" ON "AppointmentServiceVariants" ("AppointmentServiceId");

CREATE INDEX "IX_AppointmentServiceVariants_ServicePackageItemVariantId" ON "AppointmentServiceVariants" ("ServicePackageItemVariantId");

CREATE UNIQUE INDEX "IX_AppointmentSlots_AppointmentId" ON "AppointmentSlots" ("AppointmentId");

CREATE INDEX "IX_AppointmentSlots_BusinessMembershipId_StartAt_EndAt" ON "AppointmentSlots" ("BusinessMembershipId", "StartAt", "EndAt");

CREATE INDEX "IX_BookingChangeAppointments_AppointmentId" ON "BookingChangeAppointments" ("AppointmentId");

CREATE UNIQUE INDEX "IX_BookingChangeAppointments_BookingChangeRequestId_Appointmen~" ON "BookingChangeAppointments" ("BookingChangeRequestId", "AppointmentId");

CREATE UNIQUE INDEX "IX_BookingChangeRequests_BookingId" ON "BookingChangeRequests" ("BookingId") WHERE "Status" = 1;

CREATE INDEX "IX_BookingChangeRequests_BookingId_Status" ON "BookingChangeRequests" ("BookingId", "Status");

CREATE INDEX "IX_BookingChangeRequests_ExpiresAt" ON "BookingChangeRequests" ("ExpiresAt");

CREATE INDEX "IX_BookingChangeRequests_RequestedByUserId" ON "BookingChangeRequests" ("RequestedByUserId");

CREATE INDEX "IX_BookingChangeRequests_ReviewedByUserId" ON "BookingChangeRequests" ("ReviewedByUserId");

CREATE INDEX "IX_BookingHistories_BookingId_CreatedAt" ON "BookingHistories" ("BookingId", "CreatedAt");

CREATE INDEX "IX_BookingHistories_PerformedByUserId" ON "BookingHistories" ("PerformedByUserId");

CREATE INDEX "IX_Bookings_BusinessId_Status" ON "Bookings" ("BusinessId", "Status");

CREATE INDEX "IX_Bookings_BusinessMembershipId_Status" ON "Bookings" ("BusinessMembershipId", "Status");

CREATE INDEX "IX_Bookings_ExpiresAt" ON "Bookings" ("ExpiresAt");

CREATE INDEX "IX_Bookings_ReviewedByUserId" ON "Bookings" ("ReviewedByUserId");

CREATE INDEX "IX_Bookings_UserId_CreatedAt" ON "Bookings" ("UserId", "CreatedAt");

CREATE EXTENSION IF NOT EXISTS btree_gist;

ALTER TABLE "AppointmentSlots"
ADD CONSTRAINT "EX_AppointmentSlots_NoOverlap"
EXCLUDE USING gist (
    "BusinessMembershipId" WITH =,
    tstzrange("StartAt", "EndAt", '[)') WITH &&
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261026214020_AddBookingAndAppointments', '10.0.4');

COMMIT;

