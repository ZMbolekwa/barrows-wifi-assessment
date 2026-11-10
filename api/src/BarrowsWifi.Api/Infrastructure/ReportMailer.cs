
using BarrowsWifi.Api.Data;
using BarrowsWifi.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace BarrowsWifi.Api.Infrastructure;

public interface IReportMailer
{
    Task SendAsync(
        string to,
        string subject,
        string body,
        string idempotencyKey,
        CancellationToken ct);
}

public class FakeReportMailer : IReportMailer
{
    private readonly IServiceScopeFactory _scopes;
    private readonly ILogger<FakeReportMailer> _log;
    private readonly double _ackFailureRate;

    public FakeReportMailer(
        IServiceScopeFactory scopes,
        ILogger<FakeReportMailer> log,
        IConfiguration config)
    {
        _scopes = scopes;
        _log = log;
        _ackFailureRate =
            config.GetValue("Mail:AckFailureRate", 0.25);
    }

    public async Task SendAsync(
        string to,
        string subject,
        string body,
        string idempotencyKey,
        CancellationToken ct)
    {
        using var scope = _scopes.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        if (await db.SentEmails.AnyAsync(
            e => e.IdempotencyKey == idempotencyKey,
            ct))
        {
            return;
        }

        db.SentEmails.Add(new SentEmail
        {
            ToAddress = to,
            Subject = subject,
            Body = body,
            IdempotencyKey = idempotencyKey,
            SentAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync(ct);

        _log.LogInformation(
            "Email sent to {To}: {Subject}",
            to,
            subject);

        if (Random.Shared.NextDouble() < _ackFailureRate)
        {
            throw new TimeoutException(
                "Mail relay did not acknowledge the message in time.");
        }
    }
}
