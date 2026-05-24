using Koloqwa.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace Koloqwa.Infrastructure.Services;

public class ResendEmailService : IEmailService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public ResendEmailService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _apiKey = config["Resend:ApiKey"] ?? throw new InvalidOperationException("Resend:ApiKey not configured.");
        _fromEmail = config["Resend:FromEmail"] ?? "onboarding@resend.dev";
        _fromName = config["Resend:FromName"] ?? "Koloqwa Dictionary";
    }

    public async Task SendVerificationEmailAsync(
        string toEmail, string displayName, string verificationUrl, CancellationToken ct = default)
    {
        var html = $"""
            <!DOCTYPE html>
            <html>
            <body style="font-family: Georgia, serif; max-width: 560px; margin: 0 auto; padding: 40px 20px; color: #1a1a1a;">
              <div style="text-align: center; margin-bottom: 32px;">
                <div style="width: 48px; height: 48px; background: #002868; border-radius: 50%; display: inline-flex; align-items: center; justify-content: center;">
                  <span style="color: white; font-size: 20px; font-weight: bold;">K</span>
                </div>
                <h1 style="font-size: 24px; margin: 16px 0 4px; color: #002868;">Koloqwa Dictionary</h1>
              </div>
              <h2 style="font-size: 20px; font-weight: 600; margin-bottom: 12px;">Verify your email address</h2>
              <p style="color: #444; line-height: 1.6; margin-bottom: 8px;">Hi {displayName},</p>
              <p style="color: #444; line-height: 1.6; margin-bottom: 28px;">
                Thank you for joining Koloqwa — the community dictionary for Liberian language and culture.
                Please verify your email address to start contributing words and phrases.
              </p>
              <div style="text-align: center; margin-bottom: 32px;">
                <a href="{verificationUrl}"
                   style="background: #BF0A30; color: white; padding: 14px 32px; border-radius: 12px; text-decoration: none; font-weight: 600; font-size: 15px; display: inline-block;">
                  Verify my email
                </a>
              </div>
              <p style="color: #888; font-size: 13px; line-height: 1.6;">
                This link expires in 24 hours. If you did not create an account, you can safely ignore this email.
              </p>
              <hr style="border: none; border-top: 1px solid #eee; margin: 32px 0;" />
              <p style="color: #aaa; font-size: 12px; text-align: center;">
                Koloqwa Dictionary · Documenting the languages Liberians actually speak
              </p>
            </body>
            </html>
            """;

        var payload = new
        {
            from = $"{_fromName} <{_fromEmail}>",
            to = new[] { toEmail },
            subject = "Verify your Koloqwa account",
            html
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
        request.Headers.Add("Authorization", $"Bearer {_apiKey}");
        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        var response = await _http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"Resend API error: {response.StatusCode} — {body}");
        }
    }

    public async Task SendPasswordResetEmailAsync(
    string toEmail, string displayName, string resetUrl, CancellationToken ct = default)
    {
        var html = $"""
        <!DOCTYPE html>
        <html>
        <body style="font-family: Georgia, serif; max-width: 560px; margin: 0 auto; padding: 40px 20px; color: #1a1a1a;">
          <div style="text-align: center; margin-bottom: 32px;">
            <div style="width: 48px; height: 48px; background: #002868; border-radius: 50%; display: inline-flex; align-items: center; justify-content: center;">
              <span style="color: white; font-size: 20px; font-weight: bold;">K</span>
            </div>
            <h1 style="font-size: 24px; margin: 16px 0 4px; color: #002868;">Koloqwa Dictionary</h1>
          </div>
          <h2 style="font-size: 20px; font-weight: 600; margin-bottom: 12px;">Reset your password</h2>
          <p style="color: #444; line-height: 1.6; margin-bottom: 8px;">Hi {displayName},</p>
          <p style="color: #444; line-height: 1.6; margin-bottom: 28px;">
            We received a request to reset your password. Click the button below to choose a new one.
          </p>
          <div style="text-align: center; margin-bottom: 32px;">
            <a href="{resetUrl}"
               style="background: #002868; color: white; padding: 14px 32px; border-radius: 12px; text-decoration: none; font-weight: 600; font-size: 15px; display: inline-block;">
              Reset my password
            </a>
          </div>
          <p style="color: #888; font-size: 13px; line-height: 1.6;">
            This link expires in 1 hour. If you did not request a password reset, you can safely ignore this email.
          </p>
          <hr style="border: none; border-top: 1px solid #eee; margin: 32px 0;" />
          <p style="color: #aaa; font-size: 12px; text-align: center;">
            Koloqwa Dictionary · Documenting the languages Liberians actually speak
          </p>
        </body>
        </html>
        """;

        var payload = new
        {
            from = $"{_fromName} <{_fromEmail}>",
            to = new[] { toEmail },
            subject = "Reset your Koloqwa password",
            html
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
        request.Headers.Add("Authorization", $"Bearer {_apiKey}");
        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        var response = await _http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"Resend API error: {response.StatusCode} — {body}");
        }
    }
}
