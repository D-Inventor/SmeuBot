﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SmeuBase;

namespace SmeuBase.Migrations.MySQL
{
    [DbContext(typeof(SmeuContextMySQL))]
    partial class SmeuContextMySQLModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("SmeuBase.Submission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<ulong>("Author");

                    b.Property<DateTime>("Date");

                    b.Property<string>("Smeu")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("Submissions");
                });

            modelBuilder.Entity("SmeuBase.Suspension", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<ulong>("User");

                    b.HasKey("Id");

                    b.ToTable("Suspensions");
                });
#pragma warning restore 612, 618
        }
    }
}
