﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SmeuArchief.Database;

namespace SmeuArchief.Migrations
{
    [DbContext(typeof(SmeuContext))]
    [Migration("20190811105246_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity("SmeuArchief.Database.Submission", b =>
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

            modelBuilder.Entity("SmeuArchief.Database.Suspension", b =>
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
