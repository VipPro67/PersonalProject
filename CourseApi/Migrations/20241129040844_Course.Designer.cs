﻿// <auto-generated />
using System;
using CourseApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CourseApi.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20241129040844_Course")]
    partial class Course
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.18")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("CourseApi.Models.Course", b =>
                {
                    b.Property<string>("CourseId")
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)");

                    b.Property<string>("CourseName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<int?>("Credit")
                        .IsRequired()
                        .HasColumnType("integer");

                    b.Property<string>("Department")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<DateOnly?>("EndDate")
                        .IsRequired()
                        .HasColumnType("date");

                    b.Property<string>("Instructor")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("Schedule")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<DateOnly?>("StartDate")
                        .IsRequired()
                        .HasColumnType("date");

                    b.HasKey("CourseId");

                    b.ToTable("Courses");

                    b.HasData(
                        new
                        {
                            CourseId = "C001",
                            CourseName = "Introduction to C#",
                            Credit = 3,
                            Department = "Computer Science",
                            Description = "This course introduces you to the world of C# programming language and its fundamentals.",
                            EndDate = new DateOnly(2025, 2, 22),
                            Instructor = "John Doe",
                            Schedule = "9:00 AM - 12:00 PM Mon, 2:00 PM - 5:00 PM Sat",
                            StartDate = new DateOnly(2024, 9, 15)
                        },
                        new
                        {
                            CourseId = "OOP",
                            CourseName = "Object-Oriented Programming",
                            Credit = 4,
                            Department = "Computer Science",
                            Description = "This course teaches you the fundamentals of object-oriented programming in C#.",
                            EndDate = new DateOnly(2024, 5, 18),
                            Instructor = "Jane Smith",
                            Schedule = "9:00 AM - 12:00 PM Tue, 2:00 PM - 5:00 PM Wen",
                            StartDate = new DateOnly(2024, 2, 10)
                        },
                        new
                        {
                            CourseId = "IT007",
                            CourseName = "Introduction to IT Security",
                            Credit = 2,
                            Department = "Computer Science",
                            Description = "This course covers the basics of IT security and how to protect your digital assets.",
                            EndDate = new DateOnly(2025, 2, 22),
                            Instructor = "Bob Johnson",
                            Schedule = " 2:00 PM - 5:00 PM Thu",
                            StartDate = new DateOnly(2024, 9, 15)
                        });
                });
#pragma warning restore 612, 618
        }
    }
}