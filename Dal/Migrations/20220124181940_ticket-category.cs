using Microsoft.EntityFrameworkCore.Migrations;

namespace Dal.Migrations
{
    public partial class ticketcategory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReplyImages_ReplyId",
                table: "ReplyImages");

            migrationBuilder.AddColumn<string>(
                name: "Categoty",
                table: "Tickets",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ReplyImages_ReplyId",
                table: "ReplyImages",
                column: "ReplyId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReplyImages_ReplyId",
                table: "ReplyImages");

            migrationBuilder.DropColumn(
                name: "Categoty",
                table: "Tickets");

            migrationBuilder.CreateIndex(
                name: "IX_ReplyImages_ReplyId",
                table: "ReplyImages",
                column: "ReplyId");
        }
    }
}
