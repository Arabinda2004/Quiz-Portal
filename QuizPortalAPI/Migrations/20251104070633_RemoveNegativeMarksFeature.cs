using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizPortalAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNegativeMarksFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NegativeMarks",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "GradingRemark",
                table: "GradingRecords");

            migrationBuilder.DropColumn(
                name: "HasNegativeMarking",
                table: "Exams");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "NegativeMarks",
                table: "Questions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "GradingRemark",
                table: "GradingRecords",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasNegativeMarking",
                table: "Exams",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
