using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WikiQuizGenerator.Data.Migrations
{
    /// <inheritdoc />
    public partial class IListForTesting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Quizzes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AIResponses",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "WikipediaPages",
                keyColumn: "Id",
                keyValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Quizzes",
                columns: new[] { "Id", "Title" },
                values: new object[] { 1, "Test Quiz" });

            migrationBuilder.InsertData(
                table: "WikipediaPages",
                columns: new[] { "Id", "Categories", "Extract", "Langauge", "LastModified", "Length", "Links", "Title", "Url" },
                values: new object[] { 1, new[] { "Category1", "Category2" }, "This is a test Wikipedia page extract.", "en", new DateTime(2024, 7, 31, 23, 34, 51, 912, DateTimeKind.Utc).AddTicks(290), 100, new[] { "Link1", "Link2" }, "Test Wikipedia Page", "https://en.wikipedia.org/wiki/Test" });

            migrationBuilder.InsertData(
                table: "AIResponses",
                columns: new[] { "Id", "AIResponseTime", "CompletionTokenUsage", "ModelName", "PromptTokenUsage", "QuizId", "WikipediaPageId" },
                values: new object[] { 1, 1000L, 20, "GPT-4", 10, null, 1 });

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "CorrectAnswerIndex", "Options", "QuestionResponseId", "Text" },
                values: new object[] { 1, 0, new List<string> { "Option A", "Option B", "Option C", "Option D" }, 1, "What is this test question?" });
        }
    }
}
