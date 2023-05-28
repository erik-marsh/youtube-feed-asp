using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace youtube_feed_asp.Migrations
{
    public partial class CorrectionToVideoModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "LengthSeconds",
                table: "Videos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LengthSeconds",
                table: "Videos");
        }
    }
}
