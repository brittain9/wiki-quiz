﻿// <auto-generated />
using System;
using System.Collections.Generic;
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
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("WikiQuizGenerator.Core.Models.Question", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("CorrectAnswerIndex")
                        .HasColumnType("integer");

                    b.Property<List<string>>("Options")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.Property<int?>("QuestionResponseId")
                        .HasColumnType("integer");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("QuestionResponseId");

                    b.ToTable("Questions");
                });

            modelBuilder.Entity("WikiQuizGenerator.Core.Models.QuestionResponse", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<long>("AIResponseTime")
                        .HasColumnType("bigint");

                    b.Property<int?>("CompletionTokenUsage")
                        .HasColumnType("integer");

                    b.Property<string>("ModelName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("PromptTokenUsage")
                        .HasColumnType("integer");

                    b.Property<int?>("QuizId")
                        .HasColumnType("integer");

                    b.Property<string>("ResponseTopic")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("TopicUrl")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("QuizId");

                    b.ToTable("QuestionResponses");
                });

            modelBuilder.Entity("WikiQuizGenerator.Core.Models.Quiz", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Quizzes");
                });

            modelBuilder.Entity("WikiQuizGenerator.Core.Models.Question", b =>
                {
                    b.HasOne("WikiQuizGenerator.Core.Models.QuestionResponse", null)
                        .WithMany("Questions")
                        .HasForeignKey("QuestionResponseId");
                });

            modelBuilder.Entity("WikiQuizGenerator.Core.Models.QuestionResponse", b =>
                {
                    b.HasOne("WikiQuizGenerator.Core.Models.Quiz", null)
                        .WithMany("QuestionResponses")
                        .HasForeignKey("QuizId");
                });

            modelBuilder.Entity("WikiQuizGenerator.Core.Models.QuestionResponse", b =>
                {
                    b.Navigation("Questions");
                });

            modelBuilder.Entity("WikiQuizGenerator.Core.Models.Quiz", b =>
                {
                    b.Navigation("QuestionResponses");
                });
#pragma warning restore 612, 618
        }
    }
}
