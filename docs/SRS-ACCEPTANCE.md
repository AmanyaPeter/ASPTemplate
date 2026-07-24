# SRS Acceptance Matrix

| Area | Acceptance evidence |
|---|---|
| Roles | Staff, Innovation Team, IT Administrator, and Deputy Director permissions verified with positive and negative tests |
| Submission | Required fields, individual/group data, PDF/DOCX uploads, signature checks, 10 MB limit, reference generation |
| Workflow | Allowed transitions, 30/60/90/30 defaults, history, deadlines, concurrency conflict behavior |
| Tracking | Authorized details and SignalR change notification with authoritative reload |
| Notifications | In-app record, durable SMTP outbox, retries, approaching-due and overdue deduplication |
| Administration | User provisioning, activation, lock/unlock, break-glass restriction, role assignment |
| Categories/resources | CRUD, soft deactivation, authorized SQL-blob upload/download |
| Reporting | Filters, KPI totals, pagination, PDF and Excel output verified against source data |
| Audit | Authentication, submission, workflow, administration, download, report, and failure events are immutable and filterable |
| Security | HTTPS, session expiry, lockout, password policy, antiforgery, headers, throttling, OWASP/CIS review |
| Operations | IIS deployment, health check, backups, restore rehearsal, logging, alerting, rollback |
| Performance | Representative 50-user test demonstrates normal responses within five seconds and capacity is documented |
