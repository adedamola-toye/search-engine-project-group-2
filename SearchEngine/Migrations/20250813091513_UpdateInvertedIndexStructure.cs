using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SearchEngine.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInvertedIndexStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InverseDocumentFrequency",
                table: "DocumentKeywords");

            migrationBuilder.DropColumn(
                name: "TfIdfScore",
                table: "DocumentKeywords");

            migrationBuilder.AddColumn<double>(
                name: "InverseDocumentFrequency",
                table: "InvertedIndexEntries",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InverseDocumentFrequency",
                table: "InvertedIndexEntries");

            migrationBuilder.AddColumn<double>(
                name: "InverseDocumentFrequency",
                table: "DocumentKeywords",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "TfIdfScore",
                table: "DocumentKeywords",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
