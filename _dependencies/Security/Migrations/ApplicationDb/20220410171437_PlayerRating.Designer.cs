﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OverTheBoard.Data;

namespace OverTheBoard.Data.Migrations.ApplicationDb
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20220410171437_PlayerRating")]
    partial class PlayerRating
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("application")
                .HasAnnotation("ProductVersion", "5.0.8");

            modelBuilder.Entity("OverTheBoard.Data.Entities.Applications.ChessGameEntity", b =>
                {
                    b.Property<int>("GameId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Fen")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("Identifier")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("LastMoveAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("Level")
                        .HasColumnType("INTEGER");

                    b.Property<string>("NextMoveColour")
                        .HasColumnType("TEXT");

                    b.Property<int>("Period")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RoundNumber")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("StartTime")
                        .HasColumnType("TEXT");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.Property<Guid?>("TournamentId")
                        .HasColumnType("TEXT");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER");

                    b.HasKey("GameId");

                    b.ToTable("Games");
                });

            modelBuilder.Entity("OverTheBoard.Data.Entities.Applications.GameCompletionQueueEntity", b =>
                {
                    b.Property<int>("CompletionQueueId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("GameId")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsActive")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Level")
                        .HasColumnType("INTEGER");

                    b.HasKey("CompletionQueueId");

                    b.ToTable("CompletionQueue");
                });

            modelBuilder.Entity("OverTheBoard.Data.Entities.Applications.GamePlayerEntity", b =>
                {
                    b.Property<int>("PlayerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Colour")
                        .HasColumnType("TEXT");

                    b.Property<string>("ConnectionId")
                        .HasColumnType("TEXT");

                    b.Property<int>("GameId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastConnectedTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("Outcome")
                        .HasColumnType("TEXT");

                    b.Property<string>("Pgn")
                        .HasColumnType("TEXT");

                    b.Property<int>("Rating")
                        .HasColumnType("INTEGER");

                    b.Property<TimeSpan>("TimeRemaining")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("UserId")
                        .HasColumnType("TEXT");

                    b.HasKey("PlayerId");

                    b.HasIndex("GameId");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("OverTheBoard.Data.Entities.Applications.TournamentEntity", b =>
                {
                    b.Property<int>("TournamentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsActive")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("TournamentIdentifier")
                        .HasColumnType("TEXT");

                    b.HasKey("TournamentId");

                    b.ToTable("Tournaments");
                });

            modelBuilder.Entity("OverTheBoard.Data.Entities.Applications.TournamentPlayerEntity", b =>
                {
                    b.Property<int>("TournamentPlayerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("TournamentId")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("UserId")
                        .HasColumnType("TEXT");

                    b.HasKey("TournamentPlayerId");

                    b.HasIndex("TournamentId");

                    b.ToTable("TournamentPlayers");
                });

            modelBuilder.Entity("OverTheBoard.Data.Entities.Applications.TournamentQueueEntity", b =>
                {
                    b.Property<int>("TournamentQueueId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("TEXT");

                    b.Property<int>("Level")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("UserId")
                        .HasColumnType("TEXT");

                    b.HasKey("TournamentQueueId");

                    b.ToTable("TournamentQueue");
                });

            modelBuilder.Entity("OverTheBoard.Data.Entities.Applications.GamePlayerEntity", b =>
                {
                    b.HasOne("OverTheBoard.Data.Entities.Applications.ChessGameEntity", "Game")
                        .WithMany("Players")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Game");
                });

            modelBuilder.Entity("OverTheBoard.Data.Entities.Applications.TournamentPlayerEntity", b =>
                {
                    b.HasOne("OverTheBoard.Data.Entities.Applications.TournamentEntity", "Tournament")
                        .WithMany("Players")
                        .HasForeignKey("TournamentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Tournament");
                });

            modelBuilder.Entity("OverTheBoard.Data.Entities.Applications.ChessGameEntity", b =>
                {
                    b.Navigation("Players");
                });

            modelBuilder.Entity("OverTheBoard.Data.Entities.Applications.TournamentEntity", b =>
                {
                    b.Navigation("Players");
                });
#pragma warning restore 612, 618
        }
    }
}
