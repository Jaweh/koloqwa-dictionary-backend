using Koloqwa.Domain.Entities;
using Koloqwa.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Koloqwa.Infrastructure.Persistence.Seeders;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(ApplicationDbContext db, ILogger<DatabaseSeeder> logger)
    {
        _db = db; _logger = logger;
    }

    public async Task SeedAsync()
    {
        await _db.Database.MigrateAsync();

        if (!await _db.Languages.AnyAsync())
        {
            _logger.LogInformation("Seeding languages...");
            var languages = new[]
            {
                new Language { Code = "kpe", Name = "Kpelle",  Region = "Central Liberia" },
                new Language { Code = "bss", Name = "Bassa",   Region = "Central & Coastal Liberia" },
                new Language { Code = "grb", Name = "Grebo",   Region = "Southeast Liberia" },
                new Language { Code = "vai", Name = "Vai",     Region = "Northwest Liberia" },
                new Language { Code = "mnd", Name = "Mende",   Region = "Lofa County" },
                new Language { Code = "gio", Name = "Dan/Gio", Region = "Nimba County" },
                new Language { Code = "kra", Name = "Krahn",   Region = "Grand Gedeh County" },
                new Language { Code = "lom", Name = "Loma",    Region = "Lofa County" },
            };
            await _db.Languages.AddRangeAsync(languages);
            await _db.SaveChangesAsync();
        }

        if (!await _db.Users.AnyAsync(u => u.Role == UserRole.SuperAdmin))
        {
            _logger.LogInformation("Seeding SuperAdmin user...");
            var admin = new User
            {
                Email = "admin@koloqwa.lr",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123!", workFactor: 12),
                DisplayName = "Koloqwa Admin",
                Role = UserRole.SuperAdmin,
                IsActive = true,
                EmailVerified = true,
                EmailVerifiedAt = DateTime.UtcNow
            };
            _db.Users.Add(admin);
            await _db.SaveChangesAsync();
            _logger.LogInformation("SuperAdmin created: admin@koloqwa.lr / Admin@123!");
        }
    }
}
