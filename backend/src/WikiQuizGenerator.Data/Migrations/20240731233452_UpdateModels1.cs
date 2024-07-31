using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WikiQuizGenerator.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModels1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_QuestionResponses_QuestionResponseId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "ResponseTopic",
                table: "QuestionResponses");

            migrationBuilder.DropColumn(
                name: "TopicUrl",
                table: "QuestionResponses");

            migrationBuilder.AlterColumn<int>(
                name: "QuestionResponseId",
                table: "Questions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WikipediaPageId",
                table: "QuestionResponses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "WikipediaPages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Langauge = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Extract = table.Column<string>(type: "text", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    Length = table.Column<int>(type: "integer", nullable: false),
                    Links = table.Column<string[]>(type: "text[]", nullable: false),
                    Categories = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WikipediaPages", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Quizzes",
                columns: new[] { "Id", "Title" },
                values: new object[] { 1, "Test Quiz" });

            migrationBuilder.InsertData(
                table: "WikipediaPages",
                columns: new[] { "Id", "Categories", "Extract", "Langauge", "LastModified", "Length", "Links", "Title", "Url" },
                values: new object[] { 1, new[] { "Category1", "Category2" }, "This is a test Wikipedia page extract.", "en", new DateTime(2024, 7, 31, 23, 34, 51, 912, DateTimeKind.Utc).AddTicks(290), 100, new[] { "Link1", "Link2" }, "Test Wikipedia Page", "https://en.wikipedia.org/wiki/Test" });

            migrationBuilder.InsertData(
                table: "QuestionResponses",
                columns: new[] { "Id", "AIResponseTime", "CompletionTokenUsage", "ModelName", "PromptTokenUsage", "QuizId", "WikipediaPageId" },
                values: new object[] { 1, 1000L, 20, "GPT-4", 10, null, 1 });

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "CorrectAnswerIndex", "Options", "QuestionResponseId", "Text" },
                values: new object[] { 1, 0, new List<string> { "Option A", "Option B", "Option C", "Option D" }, 1, "What is this test question?" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionResponses_WikipediaPageId",
                table: "QuestionResponses",
                column: "WikipediaPageId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionResponses_WikipediaPages_WikipediaPageId",
                table: "QuestionResponses",
                column: "WikipediaPageId",
                principalTable: "WikipediaPages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_QuestionResponses_QuestionResponseId",
                table: "Questions",
                column: "QuestionResponseId",
                principalTable: "QuestionResponses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestionResponses_WikipediaPages_WikipediaPageId",
                table: "QuestionResponses");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_QuestionResponses_QuestionResponseId",
                table: "Questions");

            migrationBuilder.DropTable(
                name: "WikipediaPages");

            migrationBuilder.DropIndex(
                name: "IX_QuestionResponses_WikipediaPageId",
                table: "QuestionResponses");

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Quizzes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "QuestionResponses",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "WikipediaPageId",
                table: "QuestionResponses");

            migrationBuilder.AlterColumn<int>(
                name: "QuestionResponseId",
                table: "Questions",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "ResponseTopic",
                table: "QuestionResponses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TopicUrl",
                table: "QuestionResponses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_QuestionResponses_QuestionResponseId",
                table: "Questions",
                column: "QuestionResponseId",
                principalTable: "QuestionResponses",
                principalColumn: "Id");
        }
    }
}
