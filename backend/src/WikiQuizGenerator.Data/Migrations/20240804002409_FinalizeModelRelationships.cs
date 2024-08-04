using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WikiQuizGenerator.Data.Migrations
{
    /// <inheritdoc />
    public partial class FinalizeModelRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Quizzes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quizzes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WikipediaCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WikipediaCategories", x => x.Id);
                });

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
                name: "WikipediaPages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Language = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Extract = table.Column<string>(type: "text", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    Length = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WikipediaPages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AIResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ResponseTime = table.Column<long>(type: "bigint", nullable: false),
                    PromptTokenUsage = table.Column<int>(type: "integer", nullable: true),
                    CompletionTokenUsage = table.Column<int>(type: "integer", nullable: true),
                    ModelName = table.Column<string>(type: "text", nullable: true),
                    WikipediaPageId = table.Column<int>(type: "integer", nullable: false),
                    QuizId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIResponses_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AIResponses_WikipediaPages_WikipediaPageId",
                        column: x => x.WikipediaPageId,
                        principalTable: "WikipediaPages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WikipediaPageCategory",
                columns: table => new
                {
                    WikipediaCategoryId = table.Column<int>(type: "integer", nullable: false),
                    WikipediaPageId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WikipediaPageCategory", x => new { x.WikipediaCategoryId, x.WikipediaPageId });
                    table.ForeignKey(
                        name: "FK_WikipediaPageCategory_WikipediaCategories_WikipediaCategory~",
                        column: x => x.WikipediaCategoryId,
                        principalTable: "WikipediaCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WikipediaPageCategory_WikipediaPages_WikipediaPageId",
                        column: x => x.WikipediaPageId,
                        principalTable: "WikipediaPages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Text = table.Column<string>(type: "text", nullable: false),
                    Option1 = table.Column<string>(type: "text", nullable: false),
                    Option2 = table.Column<string>(type: "text", nullable: false),
                    Option3 = table.Column<string>(type: "text", nullable: true),
                    Option4 = table.Column<string>(type: "text", nullable: true),
                    Option5 = table.Column<string>(type: "text", nullable: true),
                    CorrectOptionNumber = table.Column<int>(type: "integer", nullable: false),
                    AIResponseId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_AIResponses_AIResponseId",
                        column: x => x.AIResponseId,
                        principalTable: "AIResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIResponses_QuizId",
                table: "AIResponses",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_AIResponses_WikipediaPageId",
                table: "AIResponses",
                column: "WikipediaPageId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_AIResponseId",
                table: "Questions",
                column: "AIResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_WikipediaPageCategory_WikipediaPageId",
                table: "WikipediaPageCategory",
                column: "WikipediaPageId");

            migrationBuilder.CreateIndex(
                name: "IX_WikipediaPageLink_WikipediaPageId",
                table: "WikipediaPageLink",
                column: "WikipediaPageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "WikipediaPageCategory");

            migrationBuilder.DropTable(
                name: "WikipediaPageLink");

            migrationBuilder.DropTable(
                name: "AIResponses");

            migrationBuilder.DropTable(
                name: "WikipediaCategories");

            migrationBuilder.DropTable(
                name: "WikipediaLinks");

            migrationBuilder.DropTable(
                name: "Quizzes");

            migrationBuilder.DropTable(
                name: "WikipediaPages");
        }
    }
}
