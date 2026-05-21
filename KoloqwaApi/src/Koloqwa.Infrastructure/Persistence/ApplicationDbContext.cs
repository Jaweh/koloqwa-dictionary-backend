using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Domain.Common;
using Koloqwa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService _currentUser;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUser) : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<WordEntry> WordEntries => Set<WordEntry>();
    public DbSet<WordDefinition> WordDefinitions => Set<WordDefinition>();
    public DbSet<WordExample> WordExamples => Set<WordExample>();
    public DbSet<PhraseEntry> PhraseEntries => Set<PhraseEntry>();
    public DbSet<PhraseMeaning> PhraseMeanings => Set<PhraseMeaning>();
    public DbSet<SubmissionQueue> SubmissionQueues => Set<SubmissionQueue>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // Auto-stamp UpdatedAt on all tracked AuditableEntities
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedBy = _currentUser.UserId;
            }
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.CreatedBy ??= _currentUser.UserId;
            }
        }

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        return await base.SaveChangesAsync(ct);
    }
}
