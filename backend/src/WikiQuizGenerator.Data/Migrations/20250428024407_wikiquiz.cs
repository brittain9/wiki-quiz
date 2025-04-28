using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WikiQuizGenerator.Data.Migrations
{
    /// <inheritdoc />
    public partial class wikiquiz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "modelId",
                table: "ModelConfigs",
                newName: "ModelId");

            migrationBuilder.RenameColumn(
                name: "MaxOutputTokens",
                table: "ModelConfigs",
                newName: "MaxTokens");

            migrationBuilder.RenameColumn(
                name: "CostPer1KOutputTokens",
                table: "ModelConfigs",
                newName: "CostPer1MOutputTokens");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ModelId",
                table: "ModelConfigs",
                newName: "modelId");

            migrationBuilder.RenameColumn(
                name: "MaxTokens",
                table: "ModelConfigs",
                newName: "MaxOutputTokens");

            migrationBuilder.RenameColumn(
                name: "CostPer1MOutputTokens",
                table: "ModelConfigs",
                newName: "CostPer1KOutputTokens");
        }
    }
}
