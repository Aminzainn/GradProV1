using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GP.Migrations
{
    /// <inheritdoc />
    public partial class UserTicket_NavigationProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserTickets_EventId",
                table: "UserTickets",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTickets_TicketTypeId",
                table: "UserTickets",
                column: "TicketTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTickets_Events_EventId",
                table: "UserTickets",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTickets_TicketTypes_TicketTypeId",
                table: "UserTickets",
                column: "TicketTypeId",
                principalTable: "TicketTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTickets_Events_EventId",
                table: "UserTickets");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTickets_TicketTypes_TicketTypeId",
                table: "UserTickets");

            migrationBuilder.DropIndex(
                name: "IX_UserTickets_EventId",
                table: "UserTickets");

            migrationBuilder.DropIndex(
                name: "IX_UserTickets_TicketTypeId",
                table: "UserTickets");
        }
    }
}
