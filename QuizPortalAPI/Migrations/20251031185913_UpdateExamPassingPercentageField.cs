using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizPortalAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExamPassingPercentageField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PassingMarks",
                table: "Exams");

            migrationBuilder.RenameColumn(
                name: "TotalMarks",
                table: "Exams",
                newName: "PassingPercentage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PassingPercentage",
                table: "Exams",
                newName: "TotalMarks");

            migrationBuilder.AddColumn<decimal>(
                name: "PassingMarks",
                table: "Exams",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
