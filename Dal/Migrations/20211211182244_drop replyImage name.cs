using Microsoft.EntityFrameworkCore.Migrations;

namespace Dal.Migrations
{
    public partial class dropreplyImagename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "ReplyImages");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ReplyImages",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
