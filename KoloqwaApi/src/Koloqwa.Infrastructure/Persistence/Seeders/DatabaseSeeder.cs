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

        await SeedLanguagesAsync();

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

    private async Task SeedLanguagesAsync()
    {
        var allLanguages = new[]
        {
            new { Code = "kpe", Name = "Kpelle",   Region = "Central Liberia" },
            new { Code = "bss", Name = "Bassa",    Region = "Central & Coastal Liberia" },
            new { Code = "grb", Name = "Grebo",    Region = "Southeast Liberia" },
            new { Code = "vai", Name = "Vai",      Region = "Northwest Liberia" },
            new { Code = "mnd", Name = "Mende",    Region = "Lofa County" },
            new { Code = "gio", Name = "Dan/Gio",  Region = "Nimba County" },
            new { Code = "kra", Name = "Krahn",    Region = "Grand Gedeh County" },
            new { Code = "lom", Name = "Loma",     Region = "Lofa County" },
            new { Code = "kru", Name = "Kru",      Region = "Sinoe & Grand Kru Counties" },
            new { Code = "lor", Name = "Lorma",    Region = "Lofa County" },
            new { Code = "kis", Name = "Kissi",    Region = "Lofa County" },
            new { Code = "gol", Name = "Gola",     Region = "Grand Cape Mount County" },
            new { Code = "man", Name = "Mandingo", Region = "Lofa & Bong Counties" },
            new { Code = "sap", Name = "Sapo",     Region = "Sinoe County" },
            new { Code = "bel", Name = "Belle",    Region = "Bong County" },
            new { Code = "dey", Name = "Dey",      Region = "Montserrado County" },
            new { Code = "mno", Name = "Mano",     Region = "Nimba County" },
            new { Code = "gbd", Name = "Gbandi",   Region = "Lofa County" },
        };

        var existingCodes = await _db.Languages
            .Select(l => l.Code)
            .ToListAsync();

        var toAdd = allLanguages
            .Where(l => !existingCodes.Contains(l.Code))
            .Select(l => new Language
            {
                Code = l.Code,
                Name = l.Name,
                Region = l.Region
            })
            .ToList();

        if (toAdd.Any())
        {
            _logger.LogInformation("Seeding {Count} missing languages...", toAdd.Count);
            await _db.Languages.AddRangeAsync(toAdd);
            await _db.SaveChangesAsync();
        }
    }
}