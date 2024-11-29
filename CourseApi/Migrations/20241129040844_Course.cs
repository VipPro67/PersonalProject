﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CourseApi.Migrations
{
    /// <inheritdoc />
    public partial class Course : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    CourseId = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CourseName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Credit = table.Column<int>(type: "integer", nullable: false),
                    Instructor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Department = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Schedule = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.CourseId);
                });

            migrationBuilder.InsertData(
                table: "Courses",
                columns: new[] { "CourseId", "CourseName", "Credit", "Department", "Description", "EndDate", "Instructor", "Schedule", "StartDate" },
                values: new object[,]
                {
                    { "C001", "Introduction to C#", 3, "Computer Science", "This course introduces you to the world of C# programming language and its fundamentals.", new DateOnly(2025, 2, 22), "John Doe", "9:00 AM - 12:00 PM Mon, 2:00 PM - 5:00 PM Sat", new DateOnly(2024, 9, 15) },
                    { "IT007", "Introduction to IT Security", 2, "Computer Science", "This course covers the basics of IT security and how to protect your digital assets.", new DateOnly(2025, 2, 22), "Bob Johnson", " 2:00 PM - 5:00 PM Thu", new DateOnly(2024, 9, 15) },
                    { "OOP", "Object-Oriented Programming", 4, "Computer Science", "This course teaches you the fundamentals of object-oriented programming in C#.", new DateOnly(2024, 5, 18), "Jane Smith", "9:00 AM - 12:00 PM Tue, 2:00 PM - 5:00 PM Wen", new DateOnly(2024, 2, 10) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Courses");
        }
    }
}
