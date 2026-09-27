using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportIssueInPristina.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIssueCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Issues",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Issues",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Issues");
        }
    }
}
