using Microsoft.EntityFrameworkCore.Migrations;

namespace Dal.Migrations
{
    public partial class isInnerReply : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsInnerReply",
                table: "Replies",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsInnerReply",
                table: "Replies");
        }
    }
}
