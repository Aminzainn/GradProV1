using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GP.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePlaceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NationalIdBackUrl",
                table: "Places",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NationalIdFrontUrl",
                table: "Places",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnershipOrRentalContractUrl",
                table: "Places",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Places",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SecurityClearanceUrl",
                table: "Places",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripePaymentLink",
                table: "Places",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NationalIdBackUrl",
                table: "Places");

            migrationBuilder.DropColumn(
                name: "NationalIdFrontUrl",
                table: "Places");

            migrationBuilder.DropColumn(
                name: "OwnershipOrRentalContractUrl",
                table: "Places");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Places");

            migrationBuilder.DropColumn(
                name: "SecurityClearanceUrl",
                table: "Places");

            migrationBuilder.DropColumn(
                name: "StripePaymentLink",
                table: "Places");
        }
    }
}
