using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GP.Migrations
{
    /// <inheritdoc />
    public partial class AddUserToPlaceFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Places",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Places_CreatedByUserId",
                table: "Places",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Places_AspNetUsers_CreatedByUserId",
                table: "Places",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Places_AspNetUsers_CreatedByUserId",
                table: "Places");

            migrationBuilder.DropIndex(
                name: "IX_Places_CreatedByUserId",
                table: "Places");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Places");
        }
    }
}
