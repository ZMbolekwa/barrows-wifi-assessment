using BarrowsWifi.Api.Data;
using BarrowsWifi.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace BarrowsWifi.Api.Infrastructure;

public record CampaignReport(
    int CampaignId,
    string CampaignName,
    string Organisation,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    int Outlets,
    int Impressions,
    int UniqueUsers,
    int SurveyCompletes);

public class ReportService
{
    private readonly AppDbContext _db;

    public ReportService(AppDbContext db) => _db = db;

    public async Task<CampaignReport?> BuildAsync(int campaignId, CancellationToken ct)
    {
    var campaign = await _db.Campaigns
        .VisibleTo(user)
        .Include(c => c.Organisation)
        .FirstOrDefaultAsync(c => c.Id == campaignId, ct);
        if (campaign is null) return null;

        // Events at the campaign's outlets during the flight
        
var events = _db.ConnectionEvents.Where(e =>
    e.CampaignId == campaignId &&
    _db.CampaignOutlets.Any(co =>
        co.CampaignId == campaignId &&
        co.OutletId == e.OutletId) &&
    e.OccurredAt >= campaign.StartsAt &&
    e.OccurredAt < campaign.EndsAt);
        var impressions = await events
          .Where(e => e.EventType == EventTypes.AdComplete)
          .Select(e => e.EventId)
           .Distinct()
           .CountAsync(ct);
        var uniqueUsers = await events
            .Where(e => e.EventType == EventTypes.AdComplete)
            .Select(e => e.DeviceHash)
            .Distinct()
            .CountAsync(ct);
        var surveys = await events.CountAsync(e => e.EventType == EventTypes.SurveyComplete, ct);
        var outlets = await _db.CampaignOutlets.CountAsync(co => co.CampaignId == campaignId, ct);

        return new CampaignReport(
            campaign.Id,
            campaign.Name,
            campaign.Organisation?.Name ?? "",
            campaign.StartsAt,
            campaign.EndsAt,
            outlets,
            impressions,
            uniqueUsers,
            surveys);
    }
}
