using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WikiQuizGenerator.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLinkClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WikipediaPageLink");

            migrationBuilder.DropTable(
                name: "WikipediaLinks");

            migrationBuilder.AddColumn<string[]>(
                name: "Links",
                table: "WikipediaPages",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Links",
                table: "WikipediaPages");

            migrationBuilder.CreateTable(
                name: "WikipediaLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WikipediaLinks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WikipediaPageLink",
                columns: table => new
                {
                    WikipediaLinkId = table.Column<int>(type: "integer", nullable: false),
                    WikipediaPageId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WikipediaPageLink", x => new { x.WikipediaLinkId, x.WikipediaPageId });
                    table.ForeignKey(
                        name: "FK_WikipediaPageLink_WikipediaLinks_WikipediaLinkId",
                        column: x => x.WikipediaLinkId,
                        principalTable: "WikipediaLinks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WikipediaPageLink_WikipediaPages_WikipediaPageId",
                        column: x => x.WikipediaPageId,
                        principalTable: "WikipediaPages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WikipediaPageLink_WikipediaPageId",
                table: "WikipediaPageLink",
                column: "WikipediaPageId");
        }
    }
}
