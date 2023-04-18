﻿// <auto-generated />
using System;
using ClangenUpdateApi.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ClangenUpdateApi.Migrations
{
    [DbContext(typeof(MainContext))]
    partial class MainContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ClangenUpdateApi.Database.Entities.Artifact", b =>
                {
                    b.Property<int>("ArtifactId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ArtifactId"));

                    b.Property<decimal>("DownloadCount")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<uint>("FileOid")
                        .HasColumnType("oid");

                    b.Property<long>("FileSize")
                        .HasColumnType("bigint");

                    b.Property<string>("GpgSignature")
                        .HasColumnType("text");

                    b.Property<bool>("HasValidSignature")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("ReleaseId")
                        .HasColumnType("integer");

                    b.HasKey("ArtifactId");

                    b.HasIndex("ReleaseId");

                    b.HasIndex("Name", "ReleaseId")
                        .IsUnique();

                    b.ToTable("Artifacts");
                });

            modelBuilder.Entity("ClangenUpdateApi.Database.Entities.Release", b =>
                {
                    b.Property<int>("ReleaseId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ReleaseId"));

                    b.Property<string>("BuildRef")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsConfirmed")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsVisible")
                        .HasColumnType("boolean");

                    b.Property<string>("VersionNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ReleaseId");

                    b.ToTable("Releases");
                });

            modelBuilder.Entity("ClangenUpdateApi.Database.Entities.ReleaseChannel", b =>
                {
                    b.Property<int>("ReleaseChannelId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ReleaseChannelId"));

                    b.Property<int?>("LatestReleaseId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ReleaseChannelId");

                    b.HasIndex("LatestReleaseId")
                        .IsUnique();

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("ReleaseChannels");
                });

            modelBuilder.Entity("ReleaseReleaseChannel", b =>
                {
                    b.Property<int>("ReleaseChannelsReleaseChannelId")
                        .HasColumnType("integer");

                    b.Property<int>("ReleasesReleaseId")
                        .HasColumnType("integer");

                    b.HasKey("ReleaseChannelsReleaseChannelId", "ReleasesReleaseId");

                    b.HasIndex("ReleasesReleaseId");

                    b.ToTable("ReleaseReleaseChannel");
                });

            modelBuilder.Entity("ClangenUpdateApi.Database.Entities.Artifact", b =>
                {
                    b.HasOne("ClangenUpdateApi.Database.Entities.Release", "Release")
                        .WithMany("Artifacts")
                        .HasForeignKey("ReleaseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Release");
                });

            modelBuilder.Entity("ClangenUpdateApi.Database.Entities.ReleaseChannel", b =>
                {
                    b.HasOne("ClangenUpdateApi.Database.Entities.Release", "LatestRelease")
                        .WithOne()
                        .HasForeignKey("ClangenUpdateApi.Database.Entities.ReleaseChannel", "LatestReleaseId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("LatestRelease");
                });

            modelBuilder.Entity("ReleaseReleaseChannel", b =>
                {
                    b.HasOne("ClangenUpdateApi.Database.Entities.ReleaseChannel", null)
                        .WithMany()
                        .HasForeignKey("ReleaseChannelsReleaseChannelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ClangenUpdateApi.Database.Entities.Release", null)
                        .WithMany()
                        .HasForeignKey("ReleasesReleaseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ClangenUpdateApi.Database.Entities.Release", b =>
                {
                    b.Navigation("Artifacts");
                });
#pragma warning restore 612, 618
        }
    }
}
