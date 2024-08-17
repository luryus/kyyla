﻿// <auto-generated />
using System;
using Kyyla.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Kyyla.Model.Migrations
{
    [DbContext(typeof(LockEventDbContext))]
    partial class LockEventDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.3");

            modelBuilder.Entity("Kyyla.Model.LockEvent", b =>
                {
                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("TEXT");

                    b.Property<int>("EventType")
                        .HasColumnType("INTEGER");

                    b.HasKey("Timestamp");

                    b.ToTable("LockEvents");
                });
#pragma warning restore 612, 618
        }
    }
}
