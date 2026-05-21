using Koloqwa.Domain.Entities;
using Koloqwa.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Koloqwa.Infrastructure.Persistence.Configurations;

public class PhraseEntryConfiguration : IEntityTypeConfiguration<PhraseEntry>
{
    public void Configure(EntityTypeBuilder<PhraseEntry> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.Slug).IsUnique();
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.Category);
        builder.HasIndex(p => new { p.Category, p.Status });

        builder.Property(p => p.PhraseText).IsRequired().HasMaxLength(500);
        builder.Property(p => p.Slug).IsRequired().HasMaxLength(550);
        builder.Property(p => p.Category)
            .HasConversion<string>()
            .HasDefaultValue(EntryCategory.Vernacular);
        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasDefaultValue(EntryStatus.PendingReview);
        builder.Property(p => p.Tags).HasColumnType("text[]");

        // LanguageId is now nullable
        builder.HasOne(p => p.Language)
            .WithMany(l => l.Phrases)
            .HasForeignKey(p => p.LanguageId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.SubmittedBy)
            .WithMany(u => u.SubmittedPhrases)
            .HasForeignKey(p => p.SubmittedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.ReviewedBy)
            .WithMany()
            .HasForeignKey(p => p.ReviewedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.Meanings)
            .WithOne(m => m.PhraseEntry)
            .HasForeignKey(m => m.PhraseEntryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
