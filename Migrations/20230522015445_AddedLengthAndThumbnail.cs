using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace youtube_feed_asp.Migrations
{
    public partial class AddedLengthAndThumbnail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Videos",
                type: "varchar(30)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "varchar(30)",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Videos",
                type: "varchar(30)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "varchar(30)");
        }
    }
}
