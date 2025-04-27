using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WikiQuizGenerator.Data.Migrations
{
    /// <inheritdoc />
    public partial class updateTokenUsage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PromptTokenUsage",
                table: "AIResponses",
                newName: "OutputTokenCount");

            migrationBuilder.RenameColumn(
                name: "CompletionTokenUsage",
                table: "AIResponses",
                newName: "InputTokenCount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OutputTokenCount",
                table: "AIResponses",
                newName: "PromptTokenUsage");

            migrationBuilder.RenameColumn(
                name: "InputTokenCount",
                table: "AIResponses",
                newName: "CompletionTokenUsage");
        }
    }
}
