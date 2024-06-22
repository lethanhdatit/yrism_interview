﻿// <auto-generated />
using EmployeeProfileManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EmployeeProfileManagement.Migrations
{
    [DbContext(typeof(EmployeeContext))]
    [Migration("20240622172111_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("EmployeeProfileManagement.Models.Employee", b =>
                {
                    b.Property<int>("EmployeeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("EmployeeId"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("EmployeeId");

                    b.ToTable("Employees");
                });

            modelBuilder.Entity("EmployeeProfileManagement.Models.Image", b =>
                {
                    b.Property<int>("ImageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ImageId"));

                    b.Property<byte[]>("Data")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.Property<int>("DisplayOrder")
                        .HasColumnType("integer");

                    b.Property<int>("ToolLanguageId")
                        .HasColumnType("integer");

                    b.HasKey("ImageId");

                    b.HasIndex("ToolLanguageId");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("EmployeeProfileManagement.Models.Position", b =>
                {
                    b.Property<int>("PositionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("PositionId"));

                    b.Property<int>("DisplayOrder")
                        .HasColumnType("integer");

                    b.Property<int>("EmployeeId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("PositionId");

                    b.HasIndex("EmployeeId");

                    b.ToTable("Positions");
                });

            modelBuilder.Entity("EmployeeProfileManagement.Models.PositionResource", b =>
                {
                    b.Property<int>("PositionResourceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("PositionResourceId"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("PositionResourceId");

                    b.ToTable("PositionResources");
                });

            modelBuilder.Entity("EmployeeProfileManagement.Models.ToolLanguage", b =>
                {
                    b.Property<int>("ToolLanguageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ToolLanguageId"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("DisplayOrder")
                        .HasColumnType("integer");

                    b.Property<int>("From")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("PositionId")
                        .HasColumnType("integer");

                    b.Property<int>("To")
                        .HasColumnType("integer");

                    b.HasKey("ToolLanguageId");

                    b.HasIndex("PositionId");

                    b.ToTable("ToolLanguages");
                });

            modelBuilder.Entity("EmployeeProfileManagement.Models.ToolLanguageResource", b =>
                {
                    b.Property<int>("ToolLanguageResourceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ToolLanguageResourceId"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("PositionResourceId")
                        .HasColumnType("integer");

                    b.HasKey("ToolLanguageResourceId");

                    b.HasIndex("PositionResourceId");

                    b.ToTable("ToolLanguageResources");
                });

            modelBuilder.Entity("EmployeeProfileManagement.Models.Image", b =>
                {
                    b.HasOne("EmployeeProfileManagement.Models.ToolLanguage", null)
                        .WithMany("Images")
                        .HasForeignKey("ToolLanguageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("EmployeeProfileManagement.Models.Position", b =>
                {
                    b.HasOne("EmployeeProfileManagement.Models.Employee", null)
                        .WithMany("Positions")
                        .HasForeignKey("EmployeeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("EmployeeProfileManagement.Models.ToolLanguage", b =>
                {
                    b.HasOne("EmployeeProfileManagement.Models.Position", null)
                        .WithMany("ToolLanguages")
                        .HasForeignKey("PositionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("EmployeeProfileManagement.Models.ToolLanguageResource", b =>
                {
                    b.HasOne("EmployeeProfileManagement.Models.PositionResource", "PositionResource")
                        .WithMany("ToolLanguageResources")
                        .HasForeignKey("PositionResourceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("PositionResource");
                });

            modelBuilder.Entity("EmployeeProfileManagement.Models.Employee", b =>
                {
                    b.Navigation("Positions");
                });

            modelBuilder.Entity("EmployeeProfileManagement.Models.Position", b =>
                {
                    b.Navigation("ToolLanguages");
                });

            modelBuilder.Entity("EmployeeProfileManagement.Models.PositionResource", b =>
                {
                    b.Navigation("ToolLanguageResources");
                });

            modelBuilder.Entity("EmployeeProfileManagement.Models.ToolLanguage", b =>
                {
                    b.Navigation("Images");
                });
#pragma warning restore 612, 618
        }
    }
}
