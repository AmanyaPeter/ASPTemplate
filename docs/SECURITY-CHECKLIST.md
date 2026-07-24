# OWASP and CIS Deployment Checklist

- Enforce HTTPS, TLS 1.2+, HSTS, approved cipher suites, certificate monitoring, and HTTP-to-HTTPS redirects.
- Run the IIS pool as a dedicated non-administrator account; remove interactive logon and unnecessary filesystem/database permissions.
- Keep Windows, IIS, .NET Hosting Bundle, SQL Server, browsers, and NuGet dependencies patched and vulnerability-scanned.
- Store connection, AD, and SMTP secrets outside source control and rotate them under Bank policy.
- Restrict SQL Server network access; require encryption; grant only required DML and migration permissions through separate identities.
- Keep antiforgery, secure/HTTP-only/SameSite cookies, 30-minute idle expiry, three-attempt lockout, CSP, clickjacking protection, MIME sniffing protection, and login throttling enabled.
- Permit only PDF and DOCX uploads up to 10 MB; validate signatures, generated storage names, hashes, authorization, and malware scanning at the infrastructure boundary.
- Review every role and permission assignment quarterly and every break-glass use immediately.
- Forward structured application, IIS, Windows security, SQL audit, and backup logs to protected centralized monitoring with retention and tamper controls.
- Disable directory browsing, WebDAV, TRACE, sample applications, unused IIS modules, detailed errors, development seeds, Swagger, and development configuration in production.
- Run authenticated OWASP testing before go-live and after material security changes.
