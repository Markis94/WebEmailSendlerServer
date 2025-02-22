﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using WebEmailSendler.Context;

#nullable disable

namespace WebEmailSendler.Migrations
{
    [DbContext(typeof(MyDbContext))]
    [Migration("20241225113157_SampleChangeDate")]
    partial class SampleChangeDate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("WebEmailSendler.Models.EmailSendResult", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("EmailSendTaskId")
                        .HasColumnType("integer");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("text");

                    b.Property<bool>("IsSuccess")
                        .HasColumnType("boolean");

                    b.Property<string>("Lschet")
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("SendDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Sum")
                        .HasColumnType("text");

                    b.Property<string>("Text")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Email");

                    b.HasIndex("EmailSendTaskId");

                    b.HasIndex("IsSuccess");

                    b.ToTable("EmailSendResults");
                });

            modelBuilder.Entity("WebEmailSendler.Models.EmailSendTask", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("BadSendCount")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("CreateDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("EndDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("HtmlMessage")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("JobId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("MaxCount")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("SendTaskStatus")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset?>("StartDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Subject")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("SuccessSendCount")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("EmailSendTask");
                });

            modelBuilder.Entity("WebEmailSendler.Models.Sample", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset>("ChangeDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("CreateDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("SampleJson")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Samles");
                });

            modelBuilder.Entity("WebEmailSendler.Models.EmailSendResult", b =>
                {
                    b.HasOne("WebEmailSendler.Models.EmailSendTask", "User")
                        .WithMany()
                        .HasForeignKey("EmailSendTaskId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });
#pragma warning restore 612, 618
        }
    }
}
