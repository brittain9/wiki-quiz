using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WikiQuizGenerator.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
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
                name: "WikipediaCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    WikipediaPageId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WikipediaCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WikipediaCategories_WikipediaPages_WikipediaPageId",
                        column: x => x.WikipediaPageId,
                        principalTable: "WikipediaPages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WikipediaLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PageName = table.Column<string>(type: "text", nullable: false),
                    WikipediaPageId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WikipediaLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WikipediaLinks_WikipediaPages_WikipediaPageId",
                        column: x => x.WikipediaPageId,
                        principalTable: "WikipediaPages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AIMetadata",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ResponseTime = table.Column<long>(type: "bigint", nullable: false),
                    PromptTokenUsage = table.Column<int>(type: "integer", nullable: true),
                    CompletionTokenUsage = table.Column<int>(type: "integer", nullable: true),
                    ModelName = table.Column<string>(type: "text", nullable: true),
                    AIResponseId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIMetadata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIMetadata_AIResponses_AIResponseId",
                        column: x => x.AIResponseId,
                        principalTable: "AIResponses",
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

            migrationBuilder.CreateTable(
                name: "WikipediaPageCategories",
                columns: table => new
                {
                    WikipediaPageId = table.Column<int>(type: "integer", nullable: false),
                    WikipediaCategoryId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WikipediaPageCategories", x => new { x.WikipediaPageId, x.WikipediaCategoryId });
                    table.ForeignKey(
                        name: "FK_WikipediaPageCategories_WikipediaCategories_WikipediaCatego~",
                        column: x => x.WikipediaCategoryId,
                        principalTable: "WikipediaCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WikipediaPageCategories_WikipediaPages_WikipediaPageId",
                        column: x => x.WikipediaPageId,
                        principalTable: "WikipediaPages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIMetadata_AIResponseId",
                table: "AIMetadata",
                column: "AIResponseId",
                unique: true);

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
                name: "IX_WikipediaCategories_WikipediaPageId",
                table: "WikipediaCategories",
                column: "WikipediaPageId");

            migrationBuilder.CreateIndex(
                name: "IX_WikipediaLinks_WikipediaPageId",
                table: "WikipediaLinks",
                column: "WikipediaPageId");

            migrationBuilder.CreateIndex(
                name: "IX_WikipediaPageCategories_WikipediaCategoryId",
                table: "WikipediaPageCategories",
                column: "WikipediaCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIMetadata");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "WikipediaLinks");

            migrationBuilder.DropTable(
                name: "WikipediaPageCategories");

            migrationBuilder.DropTable(
                name: "AIResponses");

            migrationBuilder.DropTable(
                name: "WikipediaCategories");

            migrationBuilder.DropTable(
                name: "Quizzes");

            migrationBuilder.DropTable(
                name: "WikipediaPages");
        }
    }
}
