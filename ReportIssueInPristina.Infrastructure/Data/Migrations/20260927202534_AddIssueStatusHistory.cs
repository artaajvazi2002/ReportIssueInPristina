using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportIssueInPristina.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIssueStatusHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IssueStatusUpdates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IssueId = table.Column<int>(type: "int", nullable: false),
                    PreviousStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangedByName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueStatusUpdates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssueStatusUpdates_Issues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IssueStatusUpdates_IssueId",
                table: "IssueStatusUpdates",
                column: "IssueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IssueStatusUpdates");
        }
    }
}
