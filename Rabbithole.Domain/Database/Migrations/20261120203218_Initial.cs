using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Rabbithole.Domain.Database.Migrations;

/// <inheritdoc />
public partial class _20261120203218_Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "DataProtectionKeys",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                FriendlyName = table.Column<string>(type: "text", nullable: true),
                Xml = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "EnvironmentalSettings",
            columns: table => new
            {
                Key = table.Column<string>(type: "text", nullable: false),
                Value = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EnvironmentalSettings", x => x.Key);
            });

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                NavidromeUsername = table.Column<string>(type: "text", nullable: false),
                EncryptedNavidromePassword = table.Column<string>(type: "text", nullable: false),
                ListenBrainzUsername = table.Column<string>(type: "text", nullable: true),
                DiscoverPlaylistId = table.Column<string>(type: "text", nullable: true),
                IsAdmin = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AlbumRequests",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UserId = table.Column<int>(type: "integer", nullable: false),
                ArtistName = table.Column<string>(type: "text", nullable: false),
                AlbumTitle = table.Column<string>(type: "text", nullable: false),
                DeezerAlbumId = table.Column<long>(type: "bigint", nullable: false),
                CoverUrl = table.Column<string>(type: "text", nullable: true),
                Status = table.Column<int>(type: "integer", nullable: false),
                StatusMessage = table.Column<string>(type: "text", nullable: true),
                LidarrArtistId = table.Column<int>(type: "integer", nullable: true),
                LidarrAlbumId = table.Column<int>(type: "integer", nullable: true),
                RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AlbumRequests", x => x.Id);
                table.ForeignKey(
                    name: "FK_AlbumRequests_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "BlockedArtists",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UserId = table.Column<int>(type: "integer", nullable: false),
                ArtistName = table.Column<string>(type: "text", nullable: false),
                NormalisedName = table.Column<string>(type: "text", nullable: false),
                DeezerArtistId = table.Column<long>(type: "bigint", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BlockedArtists", x => x.Id);
                table.ForeignKey(
                    name: "FK_BlockedArtists_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "UserRefreshTokens",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UserId = table.Column<int>(type: "integer", nullable: false),
                TokenHash = table.Column<string>(type: "text", nullable: false),
                ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserRefreshTokens", x => x.Id);
                table.ForeignKey(
                    name: "FK_UserRefreshTokens_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PendingPlaylistTracks",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                AlbumRequestId = table.Column<int>(type: "integer", nullable: false),
                NavidromePlaylistId = table.Column<string>(type: "text", nullable: false),
                ArtistName = table.Column<string>(type: "text", nullable: false),
                TrackTitle = table.Column<string>(type: "text", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                GaveUpAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PendingPlaylistTracks", x => x.Id);
                table.ForeignKey(
                    name: "FK_PendingPlaylistTracks_AlbumRequests_AlbumRequestId",
                    column: x => x.AlbumRequestId,
                    principalTable: "AlbumRequests",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AlbumRequests_UserId",
            table: "AlbumRequests",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_BlockedArtists_UserId",
            table: "BlockedArtists",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_PendingPlaylistTracks_AlbumRequestId",
            table: "PendingPlaylistTracks",
            column: "AlbumRequestId");

        migrationBuilder.CreateIndex(
            name: "IX_UserRefreshTokens_TokenHash",
            table: "UserRefreshTokens",
            column: "TokenHash",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_UserRefreshTokens_UserId",
            table: "UserRefreshTokens",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Users_NavidromeUsername",
            table: "Users",
            column: "NavidromeUsername",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "BlockedArtists");

        migrationBuilder.DropTable(
            name: "DataProtectionKeys");

        migrationBuilder.DropTable(
            name: "EnvironmentalSettings");

        migrationBuilder.DropTable(
            name: "PendingPlaylistTracks");

        migrationBuilder.DropTable(
            name: "UserRefreshTokens");

        migrationBuilder.DropTable(
            name: "AlbumRequests");

        migrationBuilder.DropTable(
            name: "Users");
    }
}
