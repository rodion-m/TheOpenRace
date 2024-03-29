﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using OpenRace.Data.Ef;

#nullable disable

namespace OpenRace.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20220517102506_AddParentId2")]
    partial class AddParentId2
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("OpenRace.Entities.Member", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("Age")
                        .HasColumnType("integer");

                    b.Property<Instant>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Distance")
                        .HasColumnType("integer");

                    b.Property<string>("Email")
                        .HasColumnType("text");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Gender")
                        .HasColumnType("integer");

                    b.Property<int?>("Number")
                        .HasColumnType("integer");

                    b.Property<Guid?>("ParentId")
                        .HasColumnType("uuid");

                    b.Property<string>("ParentName")
                        .HasColumnType("text");

                    b.Property<string>("PaymentId")
                        .HasColumnType("text");

                    b.Property<string>("Phone")
                        .HasColumnType("text");

                    b.Property<Duration?>("RaceResult")
                        .HasColumnType("interval");

                    b.Property<string>("Referer")
                        .HasColumnType("text");

                    b.Property<string>("RegisteredBy")
                        .HasColumnType("text");

                    b.Property<bool>("Subscribed")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.HasIndex("Email");

                    b.HasIndex("FullName");

                    b.HasIndex("PaymentId");

                    b.HasIndex("Phone");

                    b.ToTable("Members2022_5", (string)null);
                });

            modelBuilder.Entity("OpenRace.Entities.MemberResult", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("CreatedBy")
                        .HasColumnType("uuid");

                    b.Property<int>("Distance")
                        .HasColumnType("integer");

                    b.Property<int>("MemberAge")
                        .HasColumnType("integer");

                    b.Property<Guid>("MemberId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("RaceId")
                        .HasColumnType("uuid");

                    b.Property<Instant>("Result")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("Results");
                });

            modelBuilder.Entity("OpenRace.Entities.Payment", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric");

                    b.Property<string>("Hash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Instant?>("PaidAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("Hash");

                    b.HasIndex("Id");

                    b.HasIndex("PaidAt");

                    b.ToTable("Payments", (string)null);
                });

            modelBuilder.Entity("OpenRace.Entities.RaceEvent", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("CreatorName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Distance")
                        .HasColumnType("integer");

                    b.Property<int>("EventType")
                        .HasColumnType("integer");

                    b.Property<int>("MemberNumber")
                        .HasColumnType("integer");

                    b.Property<Guid>("RaceId")
                        .HasColumnType("uuid");

                    b.Property<Instant>("TimeStamp")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("Distance");

                    b.HasIndex("EventType");

                    b.HasIndex("MemberNumber");

                    b.HasIndex("RaceId");

                    b.HasIndex("TimeStamp");

                    b.ToTable("Events", (string)null);
                });

            modelBuilder.Entity("OpenRace.Entities.Member", b =>
                {
                    b.HasOne("OpenRace.Entities.Payment", "Payment")
                        .WithMany()
                        .HasForeignKey("PaymentId");

                    b.Navigation("Payment");
                });
#pragma warning restore 612, 618
        }
    }
}
