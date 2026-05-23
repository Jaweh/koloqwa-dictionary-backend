namespace Koloqwa.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string toEmail, string displayName, string verificationUrl, CancellationToken ct = default);
}
