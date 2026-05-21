using Koloqwa.Domain.Entities;
using Koloqwa.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;

namespace Koloqwa.Infrastructure.Persistence.Configurations;

public class WordEntryConfiguration : IEntityTypeConfiguration<WordEntry>
{
    public void Configure(EntityTypeBuilder<WordEntry> builder)
    {
        builder.HasKey(w => w.Id);
        builder.HasIndex(w => w.Slug).IsUnique();
        builder.HasIndex(w => w.Status);
        builder.HasIndex(w => new { w.LanguageId, w.Status });

        builder.Property(w => w.Headword).IsRequired().HasMaxLength(200);
        builder.Property(w => w.Slug).IsRequired().HasMaxLength(250);
        builder.Property(w => w.PartOfSpeech).HasConversion<string>();
        builder.Property(w => w.Status)
            .HasConversion<string>()
            .HasDefaultValue(EntryStatus.PendingReview);

        builder.Property(w => w.Tags).HasColumnType("text[]");

        builder.HasOne(w => w.Language)
            .WithMany(l => l.Words)
            .HasForeignKey(w => w.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.SubmittedBy)
            .WithMany(u => u.SubmittedWords)
            .HasForeignKey(w => w.SubmittedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(w => w.ReviewedBy)
            .WithMany()
            .HasForeignKey(w => w.ReviewedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(w => w.Definitions)
            .WithOne(d => d.WordEntry)
            .HasForeignKey(d => d.WordEntryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}