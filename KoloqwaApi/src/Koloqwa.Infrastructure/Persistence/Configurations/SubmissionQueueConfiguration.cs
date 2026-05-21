using Koloqwa.Domain.Entities;
using Koloqwa.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Koloqwa.Infrastructure.Persistence.Configurations;

public class SubmissionQueueConfiguration : IEntityTypeConfiguration<SubmissionQueue>
{
    public void Configure(EntityTypeBuilder<SubmissionQueue> builder)
    {
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => new { s.EntryType, s.Status });
        builder.HasIndex(s => s.SubmitterId);

        builder.Property(s => s.EntryType).HasConversion<string>();
        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasDefaultValue(EntryStatus.PendingReview);
        builder.Property(s => s.AdminNote).HasMaxLength(2000);

        // Submitter FK
        builder.HasOne(s => s.Submitter)
            .WithMany(u => u.Submissions)
            .HasForeignKey(s => s.SubmitterId)
            .OnDelete(DeleteBehavior.Restrict);

        // Reviewer FK
        builder.HasOne(s => s.ReviewedBy)
            .WithMany()
            .HasForeignKey(s => s.ReviewedById)
            .OnDelete(DeleteBehavior.SetNull);

        // No FK constraints on EntryId — polymorphic reference
        // resolved in application code via EntryType
        builder.Ignore(s => s.WordEntry);
        builder.Ignore(s => s.PhraseEntry);
    }
}