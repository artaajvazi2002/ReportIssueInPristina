using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportIssueInPristina.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIssueImageUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Issues");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrls",
                table: "Issues",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrls",
                table: "Issues");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Issues",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
