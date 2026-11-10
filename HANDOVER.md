Barrows WiFi Assessment – Handover Notes

1. Project Overview

This repository contains the Barrows WiFi assessment project. It includes:

* A .NET 8 Minimal API backend.
* SQL Server with Entity Framework Core.
* JWT and API key authentication.
* MassTransit messaging.
* A React-based administration portal.
* A captive portal.
* Campaign reporting and data ingestion functionality.

The assessment consists of six development tickets. Changes should be committed regularly, and this document should be updated as work progresses.

2. Ticket Progress

Ticket 1 – Security: Cross-client report access

Status: Implementation and unit-test verification completed; live API/database verification remains outstanding.

Work addressed:

* Added user context to report access.
* Applied organisation and user-level visibility restrictions.
* Enabled JWT lifetime validation and covered expired-token rejection.
* Added tests for client organisation scoping and platform-admin visibility.
* `dotnet test api/BarrowsWifi.sln`: 12 passed.
* `dotnet build api/BarrowsWifi.sln`: succeeded with 0 warnings and 0 errors.

Important files:

* api/src/BarrowsWifi.Api/Endpoints/ReportEndpoints.cs
* api/src/BarrowsWifi.Api/Infrastructure/ReportService.cs
* Related test files.

Ticket 2 – Reporting accuracy

Status: Implementation work completed; production discrepancy still requires verification.

Work addressed:

* Reviewed campaign and outlet report filtering.
* Applied campaign, outlet and campaign-date filtering.
* Used distinct event IDs for impression and survey-completion counts.
* Reviewed ingestion validation and duplicate-event handling.

Important consideration:

The reported difference between approximately 4,700 impressions and 3,600 agency impressions still needs to be demonstrated against the actual underlying data. A code change or sample test alone does not prove the root cause.

Ticket 3 – KES-212: Reliable campaign summary emails

Status: Implementation work completed; end-to-end verification outstanding.

Work addressed:

* Updated CampaignEndedConsumer to generate campaign summaries.
* Added recipient handling for client administrators.
* Introduced idempotency keys for email processing.
* Updated the fake mailer to simulate an acknowledgement timeout.
* Added database uniqueness protection for campaign summaries and email idempotency keys.

Important files:

* api/src/BarrowsWifi.Api/Messaging/CampaignEndedConsumer.cs
* api/src/BarrowsWifi.Api/Infrastructure/ReportMailer.cs
* api/src/BarrowsWifi.Api/Domain/
* api/src/BarrowsWifi.Api/Data/AppDbContext.cs
* Database migration for idempotency.

Important consideration:

The production email provider must also support idempotency keys to prevent duplicate emails when an acknowledgement is lost after successful delivery.

Ticket 4 – KES-251: Campaign wizard improvements

Status: In progress; implementation and endpoint integration need verification.

Requirements:

* Retain entered values when navigating between wizard steps.
* Display validation errors on the relevant step.
* Prevent duplicate campaign creation.
* Improve outlet selection using a paginated search endpoint.
* Test the wizard at 360px screen width and under Slow 3G conditions.

Frontend area:

* web/admin/src/features/wizard/

Backend area:

* api/src/BarrowsWifi.Api/Endpoints/

Important outstanding item:

Verify the actual campaign creation endpoint before applying server-side duplicate-submission protection. The file currently displayed as CampaignEndpoints.cs appeared to contain CampaignEndedConsumer code, so the filename and class contents need to be reconciled.

Ticket 5 – Captive portal compatibility and performance

Status: Not yet completed.

Requirements:

* Investigate blank captive portal screens on older phones.
* Investigate issues with in-app sign-in browsers.
* Improve slow initial loading.
* Prevent survey questions from appearing repeatedly.

Ticket 6 – Distributor data reconciliation

Status: Not yet completed.

Requirements:

* Investigate distributor data reconciliation.
* Implement the required CSV handling and reconciliation.
* Validate the output against the expected source data.

3. Technical Notes

* The backend uses .NET 8 Minimal APIs.
* Entity Framework Core manages database access.
* MassTransit handles campaign lifecycle messaging.
* Campaign summary generation is triggered through the CampaignEnded event.
* API database migrations are located under api/db/migrations.
* Migrations are configured to run during API startup.

4. Testing and Verification

Testing must be completed before the work is considered fully verified.

Outstanding verification includes:

* Running backend tests.
* Running frontend tests.
* Verifying report access across different client organisations through the running API and SQL Server.
* Comparing report totals against actual source data.
* Testing campaign summary email retries and duplicate prevention.
* Testing campaign wizard behaviour and responsiveness.
* Testing the captive portal on older devices and slower connections.
* Validating distributor CSV reconciliation.

Do not treat a change as verified until the relevant tests have actually been run.

5. Git and Commit History

Changes should be committed incrementally, using ticket references in commit messages.

Examples:

* KES-212: Implement reliable campaign summary emails
* KES-251: Improve campaign wizard validation and outlet selection

6. Outstanding Handover Actions

1. Verify and run tests for Tickets 1–3.
2. Complete and verify Ticket 4, including the actual campaign creation endpoint.
3. Investigate and implement Ticket 5.
4. Investigate and implement Ticket 6.
5. Update this document with final implementation details, test results and any remaining issues.

⸻

End of handover notes