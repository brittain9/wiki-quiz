﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using WikiQuizGenerator.Data;

#nullable disable

namespace WikiQuizGenerator.Data.Migrations
{
    [DbContext(typeof(WikiQuizDbContext))]
    partial class WikiQuizDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("WikiQuizGenerator.Core.Models.AIResponse", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int?>("CompletionTokenUsage")
                        .HasColumnType("integer");

                    b.Property<string>("ModelName")
                        .HasColumnType("text");

                    b.Property<int?>("PromptTokenUsage")
                        .HasColumnType("integer");

                    b.Property<int>("QuizId")
                        .HasColumnType("integer");

                    b.Property<long>("ResponseTime")
                        .HasColumnType("bigint");

                    b.Property<int>("WikipediaPageId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("QuizId");

                    b.HasIndex("WikipediaPageId");

                    b.ToTable("AIResponses");
                });

            modelBuilder.Entity("WikiQuizGenerator.Core.Models.Question", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("AIResponseId")
                        .HasColumnType("integer");

                    b.Property<int>("CorrectOptionNumber")
                        .HasColumnType("integer");

                    b.Property<string>("Option1")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Option2")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Option3")
                        .HasColumnType("text");

                    b.Property<string>("Option4")
                        .HasColumnType("text");

                    b.Property<string>("Option5")
                        .HasColumnType("text");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("AIResponseId");

                    b.ToTable("Questions");
                });

            modelBuilder.Entity("WikiQuizGenerator.Core.Models.QuestionAnswer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("QuestionId")
                        .HasColumnType("integer");

                    b.Property<int?>("QuizSubmissionId")
                        .HasColumnType("integer");

                    b.Property<int>("SelectedOptionNumber")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("QuizSubmissionId");

                    b.ToTable("QuestionAnswer");
                });

            modelBuilder.Entity("WikiQuizGenerator.Core.Models.Quiz", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Quizzes");
                });

            modelBuilder.Entity("WikiQuizGenerator.Core.Models.QuizSubmission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("QuizId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("SubmissionTime")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("QuizId");

                    b.ToTable("QuizSubmissions");
                });

            modelBuilder.Entity("WikiQuizGenerator.Core.Models.WikipediaCategory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("WikipediaCategories");
                });

            modelBuilder.Entity("WikiQuizGenerator.Core.Models.WikipediaPage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Extract")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Language")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("LastModified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Length")
                        .HasColumnType("integer");

                    b.Property<string[]>("Links")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("WikipediaId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("WikipediaPages");
                });

            modelBuilder.Entity("WikipediaPageCategory", b =>
                {
                    b.Property<int>("WikipediaCategoryId")
                        .HasColumnType("integer");

                    b.Property<int>("WikipediaPageId")
                        .HasColumnType("integer");

                    b.HasKey("WikipediaCategoryId", "WikipediaPageId");

                    b.HasIndex("WikipediaPageId");

                    b.ToTable("WikipediaPageCategory");
                });

            modelBuilder.Entity("WikiQuizGenerator.Core.Models.AIResponse", b =>
                {
                    b.HasOne("WikiQuizGenerator.Core.Models.Quiz", "Quiz")
                        .WithMany("AIResponses")
                        .HasForeignKey("QuizId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WikiQuizGenerator.Core.Models.WikipediaPage", "WikipediaPage")
                        .WithMany("AIResponses")
                        .HasForeignKey("WikipediaPageId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Quiz");

                    b.Navigation("WikipediaPage");
                });

            modelBuilder.Entity("WikiQuizGenerator.Core.Models.Question", b =>
                {
                    b.HasOne("WikiQuizGenerator.Core.Models.AIResponse", "AIResponse")
                        .WithMany("Questions")
                        .HasForeignKey("AIResponseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AIResponse");
                });

            modelBuilder.Entity("WikiQuizGenerator.Core.Models.QuestionAnswer", b =>
                {
                    b.HasOne("WikiQuizGenerator.Core.Models.QuizSubmission", null)
                        .WithMany("Answers")
                        .HasForeignKey("QuizSubmissionId");
                });

            modelBuilder.Entity("WikiQuizGenerator.Core.Models.QuizSubmission", b =>
                {
                    b.HasOne("WikiQuizGenerator.Core.Models.Quiz", "Quiz")
                        .WithMany("QuizSubmissions")
                        .HasForeignKey("QuizId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Quiz");
                });

            modelBuilder.Entity("WikipediaPageCategory", b =>
                {
                    b.HasOne("WikiQuizGenerator.Core.Models.WikipediaCategory", null)
                        .WithMany()
                        .HasForeignKey("WikipediaCategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WikiQuizGenerator.Core.Models.WikipediaPage", null)
                        .WithMany()
                        .HasForeignKey("WikipediaPageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("WikiQuizGenerator.Core.Models.AIResponse", b =>
                {
                    b.Navigation("Questions");
                });

            modelBuilder.Entity("WikiQuizGenerator.Core.Models.Quiz", b =>
                {
                    b.Navigation("AIResponses");

                    b.Navigation("QuizSubmissions");
                });

            modelBuilder.Entity("WikiQuizGenerator.Core.Models.QuizSubmission", b =>
                {
                    b.Navigation("Answers");
                });

            modelBuilder.Entity("WikiQuizGenerator.Core.Models.WikipediaPage", b =>
                {
                    b.Navigation("AIResponses");
                });
#pragma warning restore 612, 618
        }
    }
}
