﻿// <auto-generated />
using BattleTrace.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BattleTrace.Api.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20221111124155_Add_ServerScan")]
    partial class Add_ServerScan
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.10");

            modelBuilder.Entity("BattleTrace.Data.Models.Player", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT")
                        .HasColumnName("id");

                    b.Property<int>("Deaths")
                        .HasColumnType("INTEGER")
                        .HasColumnName("deaths");

                    b.Property<int>("Faction")
                        .HasColumnType("INTEGER")
                        .HasColumnName("faction");

                    b.Property<int>("Kills")
                        .HasColumnType("INTEGER")
                        .HasColumnName("kills");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("name");

                    b.Property<int>("Rank")
                        .HasColumnType("INTEGER")
                        .HasColumnName("rank");

                    b.Property<int>("Role")
                        .HasColumnType("INTEGER")
                        .HasColumnName("role");

                    b.Property<long>("Score")
                        .HasColumnType("INTEGER")
                        .HasColumnName("score");

                    b.Property<string>("ServerId")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("server_id");

                    b.Property<int>("Squad")
                        .HasColumnType("INTEGER")
                        .HasColumnName("squad");

                    b.Property<string>("Tag")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("tag");

                    b.Property<int>("Team")
                        .HasColumnType("INTEGER")
                        .HasColumnName("team");

                    b.Property<long>("UpdatedAt")
                        .HasColumnType("INTEGER")
                        .HasColumnName("updated_at");

                    b.HasKey("Id")
                        .HasName("pk_players");

                    b.HasIndex("ServerId")
                        .HasDatabaseName("ix_players_server_id");

                    b.ToTable("players", (string)null);
                });

            modelBuilder.Entity("BattleTrace.Data.Models.Server", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT")
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("name");

                    b.Property<long>("UpdatedAt")
                        .HasColumnType("INTEGER")
                        .HasColumnName("updated_at");

                    b.HasKey("Id")
                        .HasName("pk_servers");

                    b.ToTable("servers", (string)null);
                });

            modelBuilder.Entity("BattleTrace.Data.Models.ServerScan", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<int>("ServerCount")
                        .HasColumnType("INTEGER")
                        .HasColumnName("server_count");

                    b.Property<long>("Timestamp")
                        .HasColumnType("INTEGER")
                        .HasColumnName("timestamp");

                    b.HasKey("Id")
                        .HasName("pk_server_scans");

                    b.HasIndex("Timestamp")
                        .HasDatabaseName("ix_server_scans_timestamp");

                    b.ToTable("server_scans", (string)null);
                });

            modelBuilder.Entity("BattleTrace.Data.Models.Player", b =>
                {
                    b.HasOne("BattleTrace.Data.Models.Server", "Server")
                        .WithMany("Players")
                        .HasForeignKey("ServerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_players_servers_server_id");

                    b.Navigation("Server");
                });

            modelBuilder.Entity("BattleTrace.Data.Models.Server", b =>
                {
                    b.Navigation("Players");
                });
#pragma warning restore 612, 618
        }
    }
}
