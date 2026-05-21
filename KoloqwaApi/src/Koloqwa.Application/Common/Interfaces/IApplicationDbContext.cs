using Koloqwa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Koloqwa.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Language> Languages { get; }
    DbSet<WordEntry> WordEntries { get; }
    DbSet<WordDefinition> WordDefinitions { get; }
    DbSet<WordExample> WordExamples { get; }
    DbSet<PhraseEntry> PhraseEntries { get; }
    DbSet<PhraseMeaning> PhraseMeanings { get; }
    DbSet<SubmissionQueue> SubmissionQueues { get; }
    DbSet<AuditLog> AuditLogs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
