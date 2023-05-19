﻿// <auto-generated />
using System;
using BulletCLI.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BulletCLI.Migrations
{
    [DbContext(typeof(TodoContext))]
    partial class TodoContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.5");

            modelBuilder.Entity("BulletCLI.Model.Todo", b =>
                {
                    b.Property<int>("TodoId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Detail")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("TodoId");

                    b.ToTable("Todo");
                });

            modelBuilder.Entity("BulletCLI.Model.TodoEvent", b =>
                {
                    b.Property<int>("TodoEventId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateOnly>("Date")
                        .HasColumnType("TEXT");

                    b.Property<int>("EntryType")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TodoId")
                        .HasColumnType("INTEGER");

                    b.HasKey("TodoEventId");

                    b.HasIndex("TodoId");

                    b.ToTable("TodoEvents");
                });

            modelBuilder.Entity("BulletCLI.Model.TodoEvent", b =>
                {
                    b.HasOne("BulletCLI.Model.Todo", "Todo")
                        .WithMany("TodoEvents")
                        .HasForeignKey("TodoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Todo");
                });

            modelBuilder.Entity("BulletCLI.Model.Todo", b =>
                {
                    b.Navigation("TodoEvents");
                });
#pragma warning restore 612, 618
        }
    }
}
