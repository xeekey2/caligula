﻿// <auto-generated />
using System;
using Caligula.Service.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Caligula.Service.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20240806130448_AddedAccountId")]
    partial class AddedAccountId
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.32")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("Caligula.Model.DBModels.DbMap", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Maps");
                });

            modelBuilder.Entity("Caligula.Model.DBModels.DbMatch", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<int?>("Duration")
                        .HasColumnType("int");

                    b.Property<string>("Loser")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("MapId")
                        .HasColumnType("int");

                    b.Property<int>("MatchId")
                        .HasColumnType("int");

                    b.Property<string>("Winner")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("MapId");

                    b.ToTable("Matches");
                });

            modelBuilder.Entity("Caligula.Model.DBModels.DbParticipant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int>("DbMatchId")
                        .HasColumnType("int");

                    b.Property<int>("DbPlayerId")
                        .HasColumnType("int");

                    b.Property<string>("Decision")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("RatingChange")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("DbMatchId");

                    b.HasIndex("DbPlayerId");

                    b.ToTable("Participants");
                });

            modelBuilder.Entity("Caligula.Model.DBModels.DbPlayer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int>("AccountId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ProPlayerId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("Caligula.Model.DBModels.DbPlayerId", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int>("CharacterId")
                        .HasColumnType("int");

                    b.Property<int>("PlayerId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("PlayerId");

                    b.ToTable("DbPlayerId");
                });

            modelBuilder.Entity("Caligula.Model.DBModels.DbMatch", b =>
                {
                    b.HasOne("Caligula.Model.DBModels.DbMap", "Map")
                        .WithMany("Matches")
                        .HasForeignKey("MapId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Map");
                });

            modelBuilder.Entity("Caligula.Model.DBModels.DbParticipant", b =>
                {
                    b.HasOne("Caligula.Model.DBModels.DbMatch", "DbMatch")
                        .WithMany("Participants")
                        .HasForeignKey("DbMatchId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Caligula.Model.DBModels.DbPlayer", "DbPlayer")
                        .WithMany("Participants")
                        .HasForeignKey("DbPlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DbMatch");

                    b.Navigation("DbPlayer");
                });

            modelBuilder.Entity("Caligula.Model.DBModels.DbPlayerId", b =>
                {
                    b.HasOne("Caligula.Model.DBModels.DbPlayer", "Player")
                        .WithMany("CharacterIds")
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Player");
                });

            modelBuilder.Entity("Caligula.Model.DBModels.DbMap", b =>
                {
                    b.Navigation("Matches");
                });

            modelBuilder.Entity("Caligula.Model.DBModels.DbMatch", b =>
                {
                    b.Navigation("Participants");
                });

            modelBuilder.Entity("Caligula.Model.DBModels.DbPlayer", b =>
                {
                    b.Navigation("CharacterIds");

                    b.Navigation("Participants");
                });
#pragma warning restore 612, 618
        }
    }
}
