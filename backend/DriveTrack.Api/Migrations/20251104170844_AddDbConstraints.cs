using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveTrack.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDbConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM pg_class c
                    JOIN pg_namespace n ON n.oid = c.relnamespace
                    WHERE c.relkind = 'i'
                    AND c.relname = 'uq_category_owner_lower_name'
                    AND n.nspname = 'public'
                ) THEN
                    CREATE UNIQUE INDEX uq_category_owner_lower_name
                        ON ""Categories"" (""OwnerUserId"", lower(""Name""));
                END IF;
            END $$;
            ");

            migrationBuilder.Sql(@"
            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM pg_constraint WHERE conname = 'ck_fuelentry_volume_positive'
                ) THEN
                    ALTER TABLE ""FuelEntries""
                    ADD CONSTRAINT ck_fuelentry_volume_positive CHECK (""Volume"" > 0);
                END IF;
            END $$;

            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM pg_constraint WHERE conname = 'ck_fuelentry_price_nonneg'
                ) THEN
                    ALTER TABLE ""FuelEntries""
                    ADD CONSTRAINT ck_fuelentry_price_nonneg CHECK (""PricePerUnit"" >= 0);
                END IF;
            END $$;

            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM pg_constraint WHERE conname = 'ck_fuelentry_odo_nonneg'
                ) THEN
                    ALTER TABLE ""FuelEntries""
                    ADD CONSTRAINT ck_fuelentry_odo_nonneg CHECK (""OdometerKm"" >= 0);
                END IF;
            END $$;

            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM pg_constraint WHERE conname = 'ck_fuelentry_unit_allowed'
                ) THEN
                    ALTER TABLE ""FuelEntries""
                    ADD CONSTRAINT ck_fuelentry_unit_allowed CHECK (""Unit"" IN ('L', 'kWh', 'galUS', 'galUK'));
                END IF;
            END $$;
            ");

            migrationBuilder.Sql(@"
            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM pg_constraint WHERE conname = 'ck_expense_amount_positive'
                ) THEN
                    ALTER TABLE ""Expenses""
                    ADD CONSTRAINT ck_expense_amount_positive CHECK (""Amount"" > 0);
                END IF;
            END $$;

            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM pg_constraint WHERE conname = 'ck_expense_odo_nonneg_or_null'
                ) THEN
                    ALTER TABLE ""Expenses""
                    ADD CONSTRAINT ck_expense_odo_nonneg_or_null CHECK (""OdometerKm"" IS NULL OR ""OdometerKm"" >= 0);
                END IF;
            END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            DO $$
            BEGIN
                IF EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'ck_fuelentry_volume_positive') THEN
                    ALTER TABLE ""FuelEntries"" DROP CONSTRAINT ck_fuelentry_volume_positive;
                END IF;
                IF EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'ck_fuelentry_price_nonneg') THEN
                    ALTER TABLE ""FuelEntries"" DROP CONSTRAINT ck_fuelentry_price_nonneg;
                END IF;
                IF EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'ck_fuelentry_odo_nonneg') THEN
                    ALTER TABLE ""FuelEntries"" DROP CONSTRAINT ck_fuelentry_odo_nonneg;
                END IF;
                IF EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'ck_fuelentry_unit_allowed') THEN
                    ALTER TABLE ""FuelEntries"" DROP CONSTRAINT ck_fuelentry_unit_allowed;
                END IF;
            END $$;
            ");

            migrationBuilder.Sql(@"
            DO $$
            BEGIN
                IF EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'ck_expense_amount_positive') THEN
                    ALTER TABLE ""Expenses"" DROP CONSTRAINT ck_expense_amount_positive;
                END IF;
                IF EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'ck_expense_odo_nonneg_or_null') THEN
                    ALTER TABLE ""Expenses"" DROP CONSTRAINT ck_expense_odo_nonneg_or_null;
                END IF;
            END $$;
            ");

            migrationBuilder.Sql(@"
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1 FROM pg_class c
                    JOIN pg_namespace n ON n.oid = c.relnamespace
                    WHERE c.relkind = 'i'
                    AND c.relname = 'uq_category_owner_lower_name'
                    AND n.nspname = 'public'
                ) THEN
                    DROP INDEX uq_category_owner_lower_name;
                END IF;
            END $$;
            ");
        }
    }
}
