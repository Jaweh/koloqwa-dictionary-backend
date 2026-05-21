using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koloqwa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEntryCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "LanguageId",
                table: "WordEntries",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "WordEntries",
                type: "text",
                nullable: false,
                defaultValue: "Vernacular");

            migrationBuilder.AlterColumn<Guid>(
                name: "LanguageId",
                table: "PhraseEntries",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "PhraseEntries",
                type: "text",
                nullable: false,
                defaultValue: "Vernacular");

            migrationBuilder.CreateIndex(
                name: "IX_WordEntries_Category",
                table: "WordEntries",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_WordEntries_Category_Status",
                table: "WordEntries",
                columns: new[] { "Category", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PhraseEntries_Category",
                table: "PhraseEntries",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_PhraseEntries_Category_Status",
                table: "PhraseEntries",
                columns: new[] { "Category", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WordEntries_Category",
                table: "WordEntries");

            migrationBuilder.DropIndex(
                name: "IX_WordEntries_Category_Status",
                table: "WordEntries");

            migrationBuilder.DropIndex(
                name: "IX_PhraseEntries_Category",
                table: "PhraseEntries");

            migrationBuilder.DropIndex(
                name: "IX_PhraseEntries_Category_Status",
                table: "PhraseEntries");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "WordEntries");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "PhraseEntries");

            migrationBuilder.AlterColumn<Guid>(
                name: "LanguageId",
                table: "WordEntries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "LanguageId",
                table: "PhraseEntries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
