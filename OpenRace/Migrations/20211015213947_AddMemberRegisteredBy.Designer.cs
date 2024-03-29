﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using OpenRace.Data.Ef;

namespace OpenRace.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20211015213947_AddMemberRegisteredBy")]
    partial class AddMemberRegisteredBy
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.11")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("OpenRace.Entities.Member", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("Age")
                        .HasColumnType("integer");

                    b.Property<Instant>("CreatedAt")
                        .HasColumnType("timestamp");

                    b.Property<int>("Distance")
                        .HasColumnType("integer");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Gender")
                        .HasColumnType("integer");

                    b.Property<int?>("Number")
                        .HasColumnType("integer");

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

                    b.ToTable("Members2021");
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
                        .HasColumnType("timestamp");

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
                        .HasColumnType("timestamp");

                    b.HasKey("Id");

                    b.HasIndex("Hash");

                    b.HasIndex("Id");

                    b.HasIndex("PaidAt");

                    b.ToTable("Payments");
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
                        .HasColumnType("timestamp");

                    b.HasKey("Id");

                    b.HasIndex("Distance");

                    b.HasIndex("EventType");

                    b.HasIndex("MemberNumber");

                    b.HasIndex("RaceId");

                    b.HasIndex("TimeStamp");

                    b.ToTable("Events");
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
