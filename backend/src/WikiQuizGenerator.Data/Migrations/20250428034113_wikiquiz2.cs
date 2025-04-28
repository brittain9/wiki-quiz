using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WikiQuizGenerator.Data.Migrations
{
    /// <inheritdoc />
    public partial class wikiquiz2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AIResponses_ModelConfigs_ModelConfigId",
                table: "AIResponses");

            migrationBuilder.DropIndex(
                name: "IX_AIResponses_ModelConfigId",
                table: "AIResponses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AIResponses_ModelConfigId",
                table: "AIResponses",
                column: "ModelConfigId");

            migrationBuilder.AddForeignKey(
                name: "FK_AIResponses_ModelConfigs_ModelConfigId",
                table: "AIResponses",
                column: "ModelConfigId",
                principalTable: "ModelConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
