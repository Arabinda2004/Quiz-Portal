using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizPortalAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemovePasswordToAccessExam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessPassword",
                table: "Exams");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccessPassword",
                table: "Exams",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
