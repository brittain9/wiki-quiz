using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WikiQuizGenerator.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedNavigationalProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResponseTime",
                table: "AIResponses");

            migrationBuilder.AddColumn<long>(
                name: "ResponseTime",
                table: "AIMetadata",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResponseTime",
                table: "AIMetadata");

            migrationBuilder.AddColumn<long>(
                name: "ResponseTime",
                table: "AIResponses",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
