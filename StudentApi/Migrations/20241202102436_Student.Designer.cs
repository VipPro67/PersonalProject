﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using StudentApi.Data;

#nullable disable

namespace StudentApi.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20241202102436_Student")]
    partial class Student
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("StudentApi.Models.Student", b =>
                {
                    b.Property<int>("StudentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("StudentId"));

                    b.Property<string>("Address")
                        .HasColumnType("varchar(100)");

                    b.Property<DateOnly>("DateOfBirth")
                        .HasColumnType("date");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("varchar(100)");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("varchar(100)");

                    b.Property<int>("Grade")
                        .HasColumnType("integer");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("varchar(20)");

                    b.HasKey("StudentId");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("PhoneNumber")
                        .IsUnique();

                    b.ToTable("Students");

                    b.HasData(
                        new
                        {
                            StudentId = 1,
                            Address = "123 Main St",
                            DateOfBirth = new DateOnly(2003, 2, 2),
                            Email = "john.doe@example.com",
                            FullName = "Joh Doe",
                            Grade = 2,
                            PhoneNumber = "5551234567"
                        },
                        new
                        {
                            StudentId = 2,
                            Address = "456 Elm St",
                            DateOfBirth = new DateOnly(2004, 5, 9),
                            Email = "jane.smith@example.com",
                            FullName = "Jane Smith",
                            Grade = 1,
                            PhoneNumber = "1234567890"
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
