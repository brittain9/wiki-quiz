using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WikiQuizGenerator.Data.Migrations
{
    /// <inheritdoc />
    public partial class QuizSubmissionModelUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Answers",
                table: "QuizSubmissions");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "QuizSubmissions");

            migrationBuilder.CreateTable(
                name: "QuestionAnswer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuestionId = table.Column<int>(type: "integer", nullable: false),
                    SelectedOptionNumber = table.Column<int>(type: "integer", nullable: false),
                    QuizSubmissionId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionAnswer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionAnswer_QuizSubmissions_QuizSubmissionId",
                        column: x => x.QuizSubmissionId,
                        principalTable: "QuizSubmissions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionAnswer_QuizSubmissionId",
                table: "QuestionAnswer",
                column: "QuizSubmissionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestionAnswer");

            migrationBuilder.AddColumn<List<int>>(
                name: "Answers",
                table: "QuizSubmissions",
                type: "integer[]",
                nullable: false);

            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "QuizSubmissions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
