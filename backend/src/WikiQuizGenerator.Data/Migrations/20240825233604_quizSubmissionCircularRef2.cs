using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WikiQuizGenerator.Data.Migrations
{
    /// <inheritdoc />
    public partial class quizSubmissionCircularRef2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizSubmissions_Quizzes_QuizId1",
                table: "QuizSubmissions");

            migrationBuilder.DropIndex(
                name: "IX_QuizSubmissions_QuizId1",
                table: "QuizSubmissions");

            migrationBuilder.DropColumn(
                name: "QuizId1",
                table: "QuizSubmissions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuizId1",
                table: "QuizSubmissions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuizSubmissions_QuizId1",
                table: "QuizSubmissions",
                column: "QuizId1");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizSubmissions_Quizzes_QuizId1",
                table: "QuizSubmissions",
                column: "QuizId1",
                principalTable: "Quizzes",
                principalColumn: "Id");
        }
    }
}
