using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WikiQuizGenerator.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialIdentitySchema : Migration
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
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LastName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RefreshToken = table.Column<string>(type: "text", nullable: true),
                    RefreshTokenExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    NormalizedUserName = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    NormalizedEmail = table.Column<string>(type: "text", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
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
                name: "WikipediaPages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WikipediaId = table.Column<int>(type: "integer", nullable: false),
                    Language = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Extract = table.Column<string>(type: "text", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    Length = table.Column<int>(type: "integer", nullable: false),
                    Links = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WikipediaPages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuizSubmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuizId = table.Column<int>(type: "integer", nullable: false),
                    SubmissionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizSubmissions_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "QuestionAnswer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuestionId = table.Column<int>(type: "integer", nullable: false),
                    SelectedOptionNumber = table.Column<int>(type: "integer", nullable: false),
                    SubmissionId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionAnswer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionAnswer_QuizSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "QuizSubmissions",
                        principalColumn: "Id");
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
                name: "IX_QuestionAnswer_SubmissionId",
                table: "QuestionAnswer",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_AIResponseId",
                table: "Questions",
                column: "AIResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizSubmissions_QuizId",
                table: "QuizSubmissions",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_WikipediaPageCategory_WikipediaPageId",
                table: "WikipediaPageCategory",
                column: "WikipediaPageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestionAnswer");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "WikipediaPageCategory");

            migrationBuilder.DropTable(
                name: "QuizSubmissions");

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
