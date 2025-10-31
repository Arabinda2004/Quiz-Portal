using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizPortalAPI.Migrations
{
    /// <inheritdoc />
    public partial class QuestionAddProblem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PassingPercentage",
                table: "Exams");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PassingPercentage",
                table: "Exams",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
