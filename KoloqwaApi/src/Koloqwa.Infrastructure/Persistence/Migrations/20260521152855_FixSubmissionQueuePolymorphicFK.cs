using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koloqwa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixSubmissionQueuePolymorphicFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubmissionQueues_PhraseEntries_EntryId",
                table: "SubmissionQueues");

            migrationBuilder.DropForeignKey(
                name: "FK_SubmissionQueues_WordEntries_EntryId",
                table: "SubmissionQueues");

            migrationBuilder.DropIndex(
                name: "IX_SubmissionQueues_EntryId",
                table: "SubmissionQueues");

            migrationBuilder.AddColumn<Guid>(
                name: "PhraseEntryId",
                table: "SubmissionQueues",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WordEntryId",
                table: "SubmissionQueues",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionQueues_PhraseEntryId",
                table: "SubmissionQueues",
                column: "PhraseEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionQueues_WordEntryId",
                table: "SubmissionQueues",
                column: "WordEntryId");

            migrationBuilder.AddForeignKey(
                name: "FK_SubmissionQueues_PhraseEntries_PhraseEntryId",
                table: "SubmissionQueues",
                column: "PhraseEntryId",
                principalTable: "PhraseEntries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubmissionQueues_WordEntries_WordEntryId",
                table: "SubmissionQueues",
                column: "WordEntryId",
                principalTable: "WordEntries",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubmissionQueues_PhraseEntries_PhraseEntryId",
                table: "SubmissionQueues");

            migrationBuilder.DropForeignKey(
                name: "FK_SubmissionQueues_WordEntries_WordEntryId",
                table: "SubmissionQueues");

            migrationBuilder.DropIndex(
                name: "IX_SubmissionQueues_PhraseEntryId",
                table: "SubmissionQueues");

            migrationBuilder.DropIndex(
                name: "IX_SubmissionQueues_WordEntryId",
                table: "SubmissionQueues");

            migrationBuilder.DropColumn(
                name: "PhraseEntryId",
                table: "SubmissionQueues");

            migrationBuilder.DropColumn(
                name: "WordEntryId",
                table: "SubmissionQueues");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionQueues_EntryId",
                table: "SubmissionQueues",
                column: "EntryId");

            migrationBuilder.AddForeignKey(
                name: "FK_SubmissionQueues_PhraseEntries_EntryId",
                table: "SubmissionQueues",
                column: "EntryId",
                principalTable: "PhraseEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubmissionQueues_WordEntries_EntryId",
                table: "SubmissionQueues",
                column: "EntryId",
                principalTable: "WordEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
