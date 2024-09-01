using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WikiQuizGenerator.Data.Migrations
{
    /// <inheritdoc />
    public partial class updateSubmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestionAnswer_QuizSubmissions_QuizSubmissionId",
                table: "QuestionAnswer");

            migrationBuilder.RenameColumn(
                name: "QuizSubmissionId",
                table: "QuestionAnswer",
                newName: "SubmissionId");

            migrationBuilder.RenameIndex(
                name: "IX_QuestionAnswer_QuizSubmissionId",
                table: "QuestionAnswer",
                newName: "IX_QuestionAnswer_SubmissionId");

            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "QuizSubmissions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionAnswer_QuizSubmissions_SubmissionId",
                table: "QuestionAnswer",
                column: "SubmissionId",
                principalTable: "QuizSubmissions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestionAnswer_QuizSubmissions_SubmissionId",
                table: "QuestionAnswer");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "QuizSubmissions");

            migrationBuilder.RenameColumn(
                name: "SubmissionId",
                table: "QuestionAnswer",
                newName: "QuizSubmissionId");

            migrationBuilder.RenameIndex(
                name: "IX_QuestionAnswer_SubmissionId",
                table: "QuestionAnswer",
                newName: "IX_QuestionAnswer_QuizSubmissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionAnswer_QuizSubmissions_QuizSubmissionId",
                table: "QuestionAnswer",
                column: "QuizSubmissionId",
                principalTable: "QuizSubmissions",
                principalColumn: "Id");
        }
    }
}
