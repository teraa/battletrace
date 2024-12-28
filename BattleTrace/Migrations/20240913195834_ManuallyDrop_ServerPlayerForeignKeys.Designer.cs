﻿// <auto-generated />
using System;
using BattleTrace.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BattleTrace.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20240913195834_ManuallyDrop_ServerPlayerForeignKeys")]
    partial class ManuallyDrop_ServerPlayerForeignKeys
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("BattleTrace.Data.Models.Player", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text")
                        .HasColumnName("id");

                    b.Property<int>("Deaths")
                        .HasColumnType("integer")
                        .HasColumnName("deaths");

                    b.Property<int>("Faction")
                        .HasColumnType("integer")
                        .HasColumnName("faction");

                    b.Property<int>("Kills")
                        .HasColumnType("integer")
                        .HasColumnName("kills");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<string>("NormalizedName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("normalized_name");

                    b.Property<int>("Rank")
                        .HasColumnType("integer")
                        .HasColumnName("rank");

                    b.Property<int>("Role")
                        .HasColumnType("integer")
                        .HasColumnName("role");

                    b.Property<long>("Score")
                        .HasColumnType("bigint")
                        .HasColumnName("score");

                    b.Property<string>("ServerId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("server_id");

                    b.Property<int>("Squad")
                        .HasColumnType("integer")
                        .HasColumnName("squad");

                    b.Property<string>("Tag")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("tag");

                    b.Property<int>("Team")
                        .HasColumnType("integer")
                        .HasColumnName("team");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at");

                    b.HasKey("Id")
                        .HasName("pk_players");

                    b.HasIndex("NormalizedName")
                        .HasDatabaseName("ix_players_normalized_name");

                    b.HasIndex("ServerId")
                        .HasDatabaseName("ix_players_server_id");

                    b.HasIndex("Tag")
                        .HasDatabaseName("ix_players_tag");

                    b.HasIndex("UpdatedAt")
                        .HasDatabaseName("ix_players_updated_at");

                    b.ToTable("players", (string)null);
                });

            modelBuilder.Entity("BattleTrace.Data.Models.PlayerScan", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<int>("PlayerCount")
                        .HasColumnType("integer")
                        .HasColumnName("player_count");

                    b.Property<DateTimeOffset>("Timestamp")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("timestamp");

                    b.HasKey("Id")
                        .HasName("pk_player_scans");

                    b.HasIndex("Timestamp")
                        .HasDatabaseName("ix_player_scans_timestamp");

                    b.ToTable("player_scans", (string)null);
                });

            modelBuilder.Entity("BattleTrace.Data.Models.Server", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text")
                        .HasColumnName("id");

                    b.Property<string>("Country")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("country");

                    b.Property<string>("IpAddress")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("ip_address");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<int>("Port")
                        .HasColumnType("integer")
                        .HasColumnName("port");

                    b.Property<int>("TickRate")
                        .HasColumnType("integer")
                        .HasColumnName("tick_rate");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at");

                    b.HasKey("Id")
                        .HasName("pk_servers");

                    b.HasIndex("IpAddress")
                        .HasDatabaseName("ix_servers_ip_address");

                    b.HasIndex("Name")
                        .HasDatabaseName("ix_servers_name");

                    b.ToTable("servers", (string)null);
                });

            modelBuilder.Entity("BattleTrace.Data.Models.ServerScan", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<int>("ServerCount")
                        .HasColumnType("integer")
                        .HasColumnName("server_count");

                    b.Property<DateTimeOffset>("Timestamp")
                        .HasColumnType("timestamp with time zone")
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