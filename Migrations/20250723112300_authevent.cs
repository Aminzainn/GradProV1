using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GP.Migrations
{
    /// <inheritdoc />
    public partial class authevent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminNote",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CivilProtectionApprovalBackUrl",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CivilProtectionApprovalFrontUrl",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EventInsuranceUrl",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicLicenseBackUrl",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicLicenseFrontUrl",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecurityClearanceUrl",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripePaymentLink",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminNote",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "CivilProtectionApprovalBackUrl",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "CivilProtectionApprovalFrontUrl",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "EventInsuranceUrl",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "PublicLicenseBackUrl",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "PublicLicenseFrontUrl",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "SecurityClearanceUrl",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "StripePaymentLink",
                table: "Events");
        }
    }
}
