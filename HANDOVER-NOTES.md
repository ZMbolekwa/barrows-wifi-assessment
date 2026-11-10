# Handover Notes

## Ticket 1 — Cross-client report access

- The report service scopes campaign lookup through the authenticated user's organisation; platform administrators can see campaigns across organisations.
- JWT lifetime validation had been disabled in the API configuration. It is now explicitly enabled through shared `JwtOptions` validation parameters.
- Added unit tests for cross-organisation campaign visibility, platform-admin visibility, and expired JWT rejection.
- Verification: `dotnet test api/BarrowsWifi.sln` passed (12 tests); `dotnet build api/BarrowsWifi.sln` succeeded with no warnings or errors.
- Remaining verification: exercise report access across organisations through the running API with SQL Server. The tests cover the scoping query and token validation, not a live API/database integration path.

## Ticket 2 — Reporting accuracy

- Reproduced the old campaign 1 report total using the checked-in seed scripts: the prior outlet-and-date-only join returns 4,703 `ad_complete` rows.
- Of those 4,703 rows, 615 belong to campaign 2 but occur at outlets shared with campaign 1. Campaign 1 also has 442 repeated `ad_complete` rows across 350 duplicate `EventId` groups.
- Filtering by campaign 1, its outlets, and its inclusive-start/exclusive-end flight leaves 4,088 rows; counting distinct `EventId`s gives 3,646 impressions, close to the agency's approximately 3,600.
- The same retry issue affects survey events: campaign 1 has 1,195 `survey_complete` rows but 1,086 distinct `EventId`s. Survey totals now count distinct event IDs too.
- Regression tests cover campaign attribution, outlet and flight boundaries, and duplicate event IDs. `dotnet test api/BarrowsWifi.sln` passed (14 tests); `dotnet build api/BarrowsWifi.sln` succeeded with no warnings or errors.
- The source-side duplicate protection is in the existing ingest code and migration `006_dedupe_connection_events.sql`: idempotent retry handling plus a unique `EventId` index protects against concurrent duplicates, after removing historical ones.
- Remaining verification: these are generated local seed records, not production ClickHouse data. Confirm the breakdown against the production source before closing the agency's billing discrepancy.

Tickets 1 and 2 have been addressed; Tickets 3–6 remain as described in `HANDOVER.md`.
