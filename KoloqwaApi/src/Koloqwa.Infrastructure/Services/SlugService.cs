using System.Text;
using System.Text.RegularExpressions;
using Koloqwa.Application.Common.Interfaces;

namespace Koloqwa.Infrastructure.Services;

public partial class SlugService : ISlugService
{
    public string Generate(string input)
    {
        var slug = input.ToLowerInvariant().Trim();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        return slug.Trim('-');
    }

    public async Task<string> GenerateUniqueAsync(string input, Func<string, Task<bool>> existsCheck)
    {
        var baseSlug = Generate(input);
        var slug = baseSlug;
        var counter = 1;

        while (await existsCheck(slug))
            slug = $"{baseSlug}-{counter++}";

        return slug;
    }
}
