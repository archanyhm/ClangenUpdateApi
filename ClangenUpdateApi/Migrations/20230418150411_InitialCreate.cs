using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ClangenUpdateApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Releases",
                columns: table => new
                {
                    ReleaseId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VersionNumber = table.Column<string>(type: "text", nullable: false),
                    BuildRef = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Releases", x => x.ReleaseId);
                });

            migrationBuilder.CreateTable(
                name: "Artifacts",
                columns: table => new
                {
                    ArtifactId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReleaseId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    FileOid = table.Column<uint>(type: "oid", nullable: false),
                    GpgSignature = table.Column<string>(type: "text", nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    DownloadCount = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    HasValidSignature = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artifacts", x => x.ArtifactId);
                    table.ForeignKey(
                        name: "FK_Artifacts_Releases_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Releases",
                        principalColumn: "ReleaseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReleaseChannels",
                columns: table => new
                {
                    ReleaseChannelId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    LatestReleaseId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseChannels", x => x.ReleaseChannelId);
                    table.ForeignKey(
                        name: "FK_ReleaseChannels_Releases_LatestReleaseId",
                        column: x => x.LatestReleaseId,
                        principalTable: "Releases",
                        principalColumn: "ReleaseId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ReleaseReleaseChannel",
                columns: table => new
                {
                    ReleaseChannelsReleaseChannelId = table.Column<int>(type: "integer", nullable: false),
                    ReleasesReleaseId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseReleaseChannel", x => new { x.ReleaseChannelsReleaseChannelId, x.ReleasesReleaseId });
                    table.ForeignKey(
                        name: "FK_ReleaseReleaseChannel_ReleaseChannels_ReleaseChannelsReleas~",
                        column: x => x.ReleaseChannelsReleaseChannelId,
                        principalTable: "ReleaseChannels",
                        principalColumn: "ReleaseChannelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReleaseReleaseChannel_Releases_ReleasesReleaseId",
                        column: x => x.ReleasesReleaseId,
                        principalTable: "Releases",
                        principalColumn: "ReleaseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Artifacts_Name_ReleaseId",
                table: "Artifacts",
                columns: new[] { "Name", "ReleaseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Artifacts_ReleaseId",
                table: "Artifacts",
                column: "ReleaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseChannels_LatestReleaseId",
                table: "ReleaseChannels",
                column: "LatestReleaseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseChannels_Name",
                table: "ReleaseChannels",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseReleaseChannel_ReleasesReleaseId",
                table: "ReleaseReleaseChannel",
                column: "ReleasesReleaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Artifacts");

            migrationBuilder.DropTable(
                name: "ReleaseReleaseChannel");

            migrationBuilder.DropTable(
                name: "ReleaseChannels");

            migrationBuilder.DropTable(
                name: "Releases");
        }
    }
}
