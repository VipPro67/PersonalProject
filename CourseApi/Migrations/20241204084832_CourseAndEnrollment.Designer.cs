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
    [Migration("20241204084832_CourseAndEnrollment")]
    partial class CourseAndEnrollment
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("CourseApi.Models.Course", b =>
                {
                    b.Property<string>("CourseId")
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)");

                    b.Property<string>("CourseName")
                        .IsRequired()
                        .HasColumnType("varchar(100)");

                    b.Property<int?>("Credit")
                        .IsRequired()
                        .HasColumnType("integer");

                    b.Property<string>("Department")
                        .IsRequired()
                        .HasColumnType("varchar(100)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("varchar(500)");

                    b.Property<DateOnly?>("EndDate")
                        .IsRequired()
                        .HasColumnType("date");

                    b.Property<string>("Instructor")
                        .IsRequired()
                        .HasColumnType("varchar(100)");

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
                            Schedule = "2:00 PM - 5:00 PM Thu",
                            StartDate = new DateOnly(2024, 9, 15)
                        });
                });

            modelBuilder.Entity("CourseApi.Models.Enrollment", b =>
                {
                    b.Property<int>("EnrollmentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("EnrollmentId"));

                    b.Property<string>("CourseId")
                        .IsRequired()
                        .HasColumnType("character varying(10)");

                    b.Property<int?>("StudentId")
                        .IsRequired()
                        .HasColumnType("integer");

                    b.HasKey("EnrollmentId");

                    b.HasIndex("CourseId");

                    b.HasIndex("StudentId", "CourseId")
                        .IsUnique();

                    b.ToTable("Enrollments");

                    b.HasData(
                        new
                        {
                            EnrollmentId = 1,
                            CourseId = "C001",
                            StudentId = 1
                        },
                        new
                        {
                            EnrollmentId = 2,
                            CourseId = "OOP",
                            StudentId = 1
                        },
                        new
                        {
                            EnrollmentId = 3,
                            CourseId = "IT007",
                            StudentId = 1
                        },
                        new
                        {
                            EnrollmentId = 4,
                            CourseId = "C001",
                            StudentId = 2
                        },
                        new
                        {
                            EnrollmentId = 5,
                            CourseId = "OOP",
                            StudentId = 2
                        },
                        new
                        {
                            EnrollmentId = 6,
                            CourseId = "IT007",
                            StudentId = 2
                        });
                });

            modelBuilder.Entity("CourseApi.Models.Enrollment", b =>
                {
                    b.HasOne("CourseApi.Models.Course", "Course")
                        .WithMany("Enrollments")
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_Enrollment_Course");

                    b.Navigation("Course");
                });

            modelBuilder.Entity("CourseApi.Models.Course", b =>
                {
                    b.Navigation("Enrollments");
                });
#pragma warning restore 612, 618
        }
    }
}
