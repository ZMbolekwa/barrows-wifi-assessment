using System.Security.Claims;
using BarrowsWifi.Api.Data;
using BarrowsWifi.Api.Domain;
using BarrowsWifi.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BarrowsWifi.Api.Endpoints;

public record CampaignListItem(
    int Id,
    string Name,
    string Organisation,
    string Status,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    decimal BudgetZar,
    int OutletCount);

public record CampaignPagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int Total);

public record CreateCampaignRequest(
    string Name,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    decimal BudgetZar,
    IReadOnlyList<int> OutletIds,
    string? Status = null,
    int? OrganisationId = null);

public static class CampaignEndpoints
{
    public static void MapCampaignEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/campaigns")
            .WithTags("Campaigns")
            .RequireAuthorization();

        group.MapGet("/", async (
            ClaimsPrincipal principal,
            AppDbContext db,
            int? page,
            int? pageSize,
            CancellationToken ct) =>
        {
            var currentPage = page is null or < 1 ? 1 : page.Value;
            var size = pageSize is null or < 1 or > 100 ? 25 : pageSize.Value;
            var query = db.Campaigns
                .AsNoTracking()
                .VisibleTo(CurrentUser.From(principal));

            var total = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((currentPage - 1) * size)
                .Take(size)
                .Select(c => new CampaignListItem(
                    c.Id,
                    c.Name,
                    c.Organisation!.Name,
                    c.Status,
                    c.StartsAt,
                    c.EndsAt,
                    c.BudgetZar,
                    c.Outlets.Count))
                .ToListAsync(ct);

            return Results.Ok(new CampaignPagedResult<CampaignListItem>(
                items,
                currentPage,
                size,
                total));
        });

        group.MapPost("/", async (
            CreateCampaignRequest request,
            HttpRequest httpRequest,
            ClaimsPrincipal principal,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var user = CurrentUser.From(principal);
            var requestKey = httpRequest.Headers["Idempotency-Key"].FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(requestKey))
            {
                var existing = await db.Campaigns
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.CreateRequestKey == requestKey, ct);
                if (existing is not null)
                    return Results.Ok(new { id = existing.Id });
            }

            var name = request.Name?.Trim();
            var organisationId = user.IsPlatformAdmin
                ? request.OrganisationId ?? user.OrganisationId
                : user.OrganisationId;
            var outletIds = request.OutletIds?.Distinct().ToArray() ?? Array.Empty<int>();

            if (string.IsNullOrWhiteSpace(name) || name.Length > 200 ||
                request.StartsAt >= request.EndsAt || request.BudgetZar <= 0 ||
                outletIds.Length == 0 || requestKey?.Length > 100)
                return Results.BadRequest();

            var organisationExists = await db.Organisations
                .AnyAsync(o => o.Id == organisationId, ct);
            if (!organisationExists)
                return Results.BadRequest();

            var activeOutletCount = await db.Outlets
                .CountAsync(o => outletIds.Contains(o.Id) && o.IsActive, ct);
            if (activeOutletCount != outletIds.Length)
                return Results.BadRequest();

            var now = DateTimeOffset.UtcNow;
            var campaign = new Campaign
            {
                OrganisationId = organisationId,
                Name = name,
                Status = CampaignStatus.Draft,
                StartsAt = request.StartsAt,
                EndsAt = request.EndsAt,
                BudgetZar = request.BudgetZar,
                CreateRequestKey = string.IsNullOrWhiteSpace(requestKey) ? null : requestKey,
                CreatedAt = now,
                UpdatedAt = now,
                Outlets = outletIds.Select(id => new CampaignOutlet { OutletId = id }).ToList()
            };

            db.Campaigns.Add(campaign);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/campaigns/{campaign.Id}", new { id = campaign.Id });
        })
        .RequireAuthorization("CanEditCampaigns");
    }
}
