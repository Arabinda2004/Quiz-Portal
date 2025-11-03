using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QuizPortalAPI.Migrations
{
    /// <inheritdoc />
    public partial class QuestionPoolAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ExamID",
                table: "Questions",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "QuestionPoolID",
                table: "Questions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "QuestionPools",
                columns: table => new
                {
                    QuestionPoolID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    PoolName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionPools", x => x.QuestionPoolID);
                    table.ForeignKey(
                        name: "FK_QuestionPools_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_QuestionPoolID",
                table: "Questions",
                column: "QuestionPoolID");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionPools_CreatedBy",
                table: "QuestionPools",
                column: "CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_QuestionPools_QuestionPoolID",
                table: "Questions",
                column: "QuestionPoolID",
                principalTable: "QuestionPools",
                principalColumn: "QuestionPoolID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_QuestionPools_QuestionPoolID",
                table: "Questions");

            migrationBuilder.DropTable(
                name: "QuestionPools");

            migrationBuilder.DropIndex(
                name: "IX_Questions_QuestionPoolID",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "QuestionPoolID",
                table: "Questions");

            migrationBuilder.AlterColumn<int>(
                name: "ExamID",
                table: "Questions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
