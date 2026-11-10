# Handover Notes

## Ticket 1 — Cross-client report access

- The report service scopes campaign lookup through the authenticated user's organisation; platform administrators can see campaigns across organisations.
- JWT lifetime validation had been disabled in the API configuration. It is now explicitly enabled through shared `JwtOptions` validation parameters.
- Added unit tests for cross-organisation campaign visibility, platform-admin visibility, and expired JWT rejection.
- Verification: `dotnet test api/BarrowsWifi.sln` passed (12 tests); `dotnet build api/BarrowsWifi.sln` succeeded with no warnings or errors.
- Remaining verification: exercise report access across organisations through the running API with SQL Server. The tests cover the scoping query and token validation, not a live API/database integration path.

Only Ticket 1 was selected for this work; Tickets 2–6 remain as described in `HANDOVER.md`.
