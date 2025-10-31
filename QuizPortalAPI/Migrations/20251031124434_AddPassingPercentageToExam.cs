using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizPortalAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPassingPercentageToExam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PassingPercentage",
                table: "Exams",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PassingPercentage",
                table: "Exams");
        }
    }
}
