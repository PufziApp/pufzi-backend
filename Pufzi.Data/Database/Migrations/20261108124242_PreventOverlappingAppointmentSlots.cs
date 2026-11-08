using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pufzi.Data.Database.Migrations
{
    /// <summary>
    /// Previne suprapunerea programărilor pentru același angajat.
    /// </summary>
    public partial class PreventOverlappingAppointmentSlots : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE EXTENSION IF NOT EXISTS btree_gist;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conname = 'EX_AppointmentSlots_NoOverlap'
                          AND conrelid = 'public."AppointmentSlots"'::regclass
                    ) THEN
                        ALTER TABLE public."AppointmentSlots"
                        ADD CONSTRAINT "EX_AppointmentSlots_NoOverlap"
                        EXCLUDE USING gist
                        (
                            "BusinessMembershipId" WITH =,
                            tstzrange("StartAt", "EndAt", '[)') WITH &&
                        );
                    END IF;
                END
                $$;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE public."AppointmentSlots"
                DROP CONSTRAINT IF EXISTS "EX_AppointmentSlots_NoOverlap";
                """);
        }
    }
}