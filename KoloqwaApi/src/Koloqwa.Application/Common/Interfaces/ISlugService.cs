namespace Koloqwa.Application.Common.Interfaces;

public interface ISlugService
{
    string Generate(string input);
    Task<string> GenerateUniqueAsync(string input, Func<string, Task<bool>> existsCheck);
}
