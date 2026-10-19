using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MechanicShop.infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UniqueNormalizedLicensePlate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Bring existing plates into the form Car.NormalizeLicensePlate produces (uppercase, no whitespace).
            migrationBuilder.Sql("""
                UPDATE Vehicles
                SET LicensePlate = UPPER(REPLACE(REPLACE(REPLACE(REPLACE(LicensePlate, ' ', ''), CHAR(9), ''), CHAR(10), ''), CHAR(13), ''));
                """);

            // Refuse to guess which car keeps a shared plate; the data must be cleaned up by hand first.
            // THROW aborts the migration and its transaction rolls back the normalization above.
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM Vehicles GROUP BY LicensePlate HAVING COUNT(*) > 1)
                    THROW 50001, 'Duplicate license plates exist in Vehicles (after removing spaces and uppercasing). Resolve them before applying migration UniqueNormalizedLicensePlate.', 1;

                IF EXISTS (SELECT 1 FROM Vehicles WHERE LEN(LicensePlate) > 20)
                    THROW 50002, 'License plates longer than 20 characters exist in Vehicles. Shorten them before applying migration UniqueNormalizedLicensePlate.', 1;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "LicensePlate",
                table: "Vehicles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_LicensePlate",
                table: "Vehicles",
                column: "LicensePlate",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vehicles_LicensePlate",
                table: "Vehicles");

            migrationBuilder.AlterColumn<string>(
                name: "LicensePlate",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);
        }
    }
}
