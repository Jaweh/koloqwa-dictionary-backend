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

        builder.Property(s => s.EntryType).HasConversion<string>();
        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasDefaultValue(EntryStatus.PendingReview);
        builder.Property(s => s.AdminNote).HasMaxLength(2000);

        builder.HasOne(s => s.Submitter)
            .WithMany(u => u.Submissions)
            .HasForeignKey(s => s.SubmitterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.ReviewedBy)
            .WithMany()
            .HasForeignKey(s => s.ReviewedById)
            .OnDelete(DeleteBehavior.SetNull);

        // Polymorphic soft-navigation
        builder.HasOne(s => s.WordEntry)
            .WithMany(w => w.Submissions)
            .HasForeignKey(s => s.EntryId)
            .HasPrincipalKey(w => w.Id)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.PhraseEntry)
            .WithMany(p => p.Submissions)
            .HasForeignKey(s => s.EntryId)
            .HasPrincipalKey(p => p.Id)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
