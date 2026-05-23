using Koloqwa.Application.Common.Interfaces;
using Koloqwa.Application.Common.Interfaces.Repositories;
using Koloqwa.Infrastructure.Identity;
using Koloqwa.Infrastructure.Persistence;
using Koloqwa.Infrastructure.Persistence.Repositories;
using Koloqwa.Infrastructure.Persistence.Seeders;
using Koloqwa.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Koloqwa.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());

        // Repositories
        services.AddScoped<IWordRepository, WordRepository>();
        services.AddScoped<IPhraseRepository, PhraseRepository>();

        // Services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ISlugService, SlugService>();
        services.AddScoped<IAuditService, AuditService>();

        // Email service
        services.AddHttpClient<IEmailService, ResendEmailService>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}