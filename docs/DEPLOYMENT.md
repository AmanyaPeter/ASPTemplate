# IMTS Production Deployment and Operations

## Prerequisites

- Supported Windows Server with current security updates.
- IIS with the ASP.NET Core Hosting Bundle for .NET 8.
- SQL Server with encrypted connections, a dedicated database, and a least-privilege application login.
- A domain service account for the IIS application pool and access to Active Directory and the internal SMTP relay.
- A trusted TLS certificate whose subject matches the production host name.

## Configuration and deployment

1. Back up and restore-test the production database before every release.
2. Publish with `dotnet publish Template.Web/Template.Web.csproj -c Release -o publish`.
3. Create an IIS application pool with **No Managed Code**, 64-bit enabled, and the approved domain service identity.
4. Deploy the publish output and grant the application-pool identity read/execute access. Grant write access only to the configured log directory.
5. Bind HTTPS, redirect HTTP to HTTPS, require TLS 1.2 or later, and retain the supplied `web.config`.
6. Set secrets through IIS environment variables or the approved secret-management facility:
   - `ConnectionStrings__DefaultConnection`
   - `ActiveDirectory__Domain` and `ActiveDirectory__Container`
   - `Smtp__Host`, `Smtp__Port`, `Smtp__FromAddress`, and any relay credentials
7. Apply `Data/IMTS_Database.sql` through the controlled DBA release process.
8. Start the site, check `/health/live`, authenticate with each role, submit an idea, review it, download a file, generate a report, and verify notification/outbox processing.

Rollback consists of stopping the site, restoring the prior application package, and following the reviewed migration rollback or database-restore runbook. Never execute an automatic schema downgrade in production.

## Backup and recovery

- Use SQL Server full backups daily, differential backups at least every six hours, and transaction-log backups at least every 15 minutes when the database uses Full recovery.
- Encrypt backups, store copies on separate protected infrastructure, restrict restore permissions, and monitor every failed backup.
- Retain according to the Bank's records policy. Business owners must approve final RPO and RTO values.
- Perform a documented restore test at least quarterly, including attachment blob integrity checks and application smoke tests.

## Monitoring

Monitor IIS availability, request latency/error rate, SQL connectivity/capacity, failed logins, locked accounts, email outbox age/failures, reminder-worker execution, disk space, certificate expiry, and backup age. Alert before the 99.5% monthly availability objective is threatened.
