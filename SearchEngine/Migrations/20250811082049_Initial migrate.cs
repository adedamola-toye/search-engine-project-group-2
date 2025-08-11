using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SearchEngine.Migrations
{
    /// <inheritdoc />
    public partial class Initialmigrate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IndexedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsIndexed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentKeywords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Term = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NormalizedTerm = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Frequency = table.Column<int>(type: "int", nullable: false),
                    TermFrequency = table.Column<double>(type: "float", nullable: false),
                    InverseDocumentFrequency = table.Column<double>(type: "float", nullable: false),
                    TfIdfScore = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentKeywords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentKeywords_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvertedIndexEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentKeywordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NormalizedTerm = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TfIdfScore = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvertedIndexEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvertedIndexEntries_DocumentKeywords_DocumentKeywordId",
                        column: x => x.DocumentKeywordId,
                        principalTable: "DocumentKeywords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentKeywords_DocumentId",
                table: "DocumentKeywords",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_InvertedIndexEntries_DocumentKeywordId",
                table: "InvertedIndexEntries",
                column: "DocumentKeywordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvertedIndexEntries");

            migrationBuilder.DropTable(
                name: "DocumentKeywords");

            migrationBuilder.DropTable(
                name: "Documents");
        }
    }
}
